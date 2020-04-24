$jsonPath = "$(Split-Path -Parent $PSCommandPath)/BuildIntlData.config.json"
$config = ConvertFrom-Json ("" + (Get-Content $jsonPath -ErrorAction Silent))
if (-not $config) {
    $config = @{}
}
if (-not $config.cldrRepository) {
    $config.cldrRepository = Read-Host 'Path of CLDR repository'
    ConvertTo-Json $config > $jsonPath
}

$cldrFolder = $config.cldrRepository
$projFolder = Join-Path $PSCommandPath -ChildPath "../../src/Codeless.Ecma.Intl" -Resolve
if (-not $cldrFolder -or -not (Test-Path -Path $cldrFolder)) {
    Write-Host "Usage: ./$(Split-Path -Leaf $PSCommandPath) <cldr-repos-path>"
    return
}

$7zipPath = "$env:ProgramFiles/7-Zip/7z.exe"
if (Test-Path -Path $7zipPath -PathType Leaf) {
    Set-Alias 7zip $7zipPath
}

try {
    [Reflection.Assembly]::LoadFile("$projFolder/bin/Debug/net45/TimeZoneConverter.dll") | Out-Null
    [Reflection.Assembly]::LoadFile("$projFolder/bin/Debug/net45/Codeless.Ecma.dll") | Out-Null
    [Reflection.Assembly]::LoadFile("$projFolder/bin/Debug/net45/Codeless.Ecma.Intl.dll") | Out-Null
} catch {
    Write-Host "Build the solution before running this script." -ForegroundColor Red
    return
}
$BcpLanguageTag = [Codeless.Ecma.Intl.Utilities.BcpLanguageTag]
$IntlUtility = [Codeless.Ecma.Intl.Utilities.IntlUtility]
$CldrUtility = $IntlUtility.Assembly.GetType("Codeless.Ecma.Intl.Utilities.CldrUtility")
$placeholder = $CldrUtility::InheritedValuePlaceholder

$localeAttrs = @(
    '/supplementalData/plurals/pluralRules/@locales',
    '/supplementalData/parentLocales/parentLocale/@parent',
    '/supplementalData/parentLocales/parentLocale/@locales',
    '/supplementalData/metadata/defaultContent/@locales'
)
$extensions = @(
    @{ type='ca'; path='/root/calendars/calendar/@type' },
    @{ type='ca'; path='/supplementalData/calendar/@type' },
    @{ type='ca'; path='/supplementalData/calendarPreferenceData/calendarPreference/@ordering' }
)
$patterns = @(
    '/root/calendars/calendar/eras/*',
    '/root/calendars/calendar/dateFormats/dateFormatLength',
    '/root/calendars/calendar/timeFormats/timeFormatLength',
    '/root/calendars/calendar/dateTimeFormats/dateTimeFormatLength',
    '/root/calendars/calendar/dateTimeFormats/availableFormats',
    '/root/calendars/calendar/dateTimeFormats/intervalFormats/intervalFormatItem',
    '/root/calendars/calendar/dateTimeFormats/intervalFormats/intervalFormatFallback',
    '/root/calendars/calendar/months/monthContext/monthWidth',
    '/root/calendars/calendar/days/dayContext/dayWidth',
    '/root/calendars/calendar/dayPeriods/dayPeriodContext/dayPeriodWidth',
    '/root/calendars/calendar/monthPatterns/monthPatternContext/monthPatternWidth',
    '/root/calendars/calendar/cyclicNameSets/cyclicNameSet/cyclicNameContext/cyclicNameWidth',
    '/root/fields/field/relativeTime',
    '/root/fields/field',
    '/root/units/unitLength/compoundUnit',
    '/root/units/unitLength/unit',
    '/root/listPatterns/listPattern',
    '/root/timeZoneNames/*',
    '/root/numbers/symbols',
    '/root/numbers/decimalFormats/decimalFormatLength/decimalFormat',
    '/root/numbers/decimalFormats',
    '/root/numbers/percentFormats',
    '/root/numbers/scientificFormats',
    '/root/numbers/currencyFormats/currencyFormatLength/currencyFormat',
    '/root/numbers/currencyFormats',
    '/root/numbers/minimalPairs',
    '/root/numbers/miscPatterns'
)

function AddAttribute($elm, $name, $value) {
    $attr = $elm.OwnerDocument.CreateAttribute($name)
    $attr.Value = $value
    $elm.Attributes.Append($attr) | Out-Null
}

function RemoveNodes($node, $path) {
    $node = @($node)
    if ($path) {
        $node = @($node | % { $_.SelectNodes($path) })
    }
    $node | % {
        if ($_ -is [Xml.XmlAttribute]) {
            $_.OwnerElement.Attributes.Remove($_)
        } else {
            $_.ParentNode.RemoveChild($_)
        }
    } | Out-Null
}

function CanonicalizeLocales($doc) {
    $localeAttrs | % {
        $doc.SelectNodes($_) | % {
            $_.Value = ($_.Value -split '\s+' | ? { $_ } | % {
                $IntlUtility::CanonicalizeLanguageTag($_)
            }) -join ' '
        }
    }
}

function CanonicalizeExtensionValues($doc) {
    $extensions | % {
        $entry = $_
        $doc.SelectNodes($_.path) | % {
            $_.Value = ($_.Value -split '\s+' | ? { $_ } | % {
                $BcpLanguageTag::GetCanonicalExtensionValue($entry.type, $_)
            }) -join ' '
        }
    }
}

function TransformValues($doc, $path, $callback) {
    $doc.SelectNodes($path) | % {
        if ($_ -is [Xml.XmlElement]) {
            $_.FirstChild.Value = Invoke-Command -ScriptBlock $callback -InputObject $_.FirstChild.Value
        } elseif ($_ -is [Xml.XmlAttribute]) {
            $_.Value = Invoke-Command -ScriptBlock $callback -InputObject $_.Value
        }
    }
}

function WriteXmlAndGZ($doc, $path) {
    $tmp = [IO.Path]::GetTempFileName()
    $fs = New-Object IO.StreamWriter($tmp, $false, [Text.Encoding]::UTF8)
    $settings = New-Object Xml.XmlWriterSettings
    $settings.Indent = $true
    $settings.Encoding = [Text.Encoding]::UTF8
    $writer = [Xml.XmlTextWriter]::Create($fs, $settings)
    $doc.WriteTo($writer)
    $writer.Flush()
    $fs.Flush()
    $fs.Close()
    
    if (-not (Test-Path -Path $path -PathType Leaf) -or 
        (-not (Test-Path -Path "$path.gz" -PathType Leaf)) -or 
        ((Get-FileHash $tmp).Hash -ne (Get-FileHash $path).Hash)) {
        Write-Host "Generating GZip file $(Split-Path $path -Leaf).gz" -ForegroundColor Yellow
        cp $tmp $path
        if ([System.Environment]::OSVersion.Platform -eq 'Unix') {
            gzip -7 -f "$path"
        } elseif (Get-Command 7zip -ErrorAction Silent) {
            7zip a -mx=7 "$path.gz" $path
        } else {
            $fs = [IO.File]::OpenWrite("$path.gz")
            $gz = New-Object IO.Compression.GZipStream($fs, [IO.Compression.CompressionLevel]"Optimal")
            $writer = [Xml.XmlTextWriter]::Create($gz)
            $doc.WriteTo($writer)
            $writer.Flush()
            $writer.Close()
            $gz.Flush()
            $gz.Close()
        }
        Write-Host "$(Split-Path $path -Leaf).gz created with " (Get-Item "$path.gz").Length "bytes" -ForegroundColor Yellow
    } else {
        Write-Host "$(Split-Path $path -Leaf) is up-to-date"
    }
    rm $tmp
}

function AreAllPatternsInherited($node) {
    @($node.SelectNodes('descendant-or-self::*[count(*) = 0]') | ? { $_.InnerText -ne $placeholder }).Count -eq 0
}

function ExtractMain($xpath, $filename, $preprocess) {
    $count = 1
    $doc = New-Object Xml.XmlDocument
    $doc.AppendChild($doc.CreateElement('root')) | Out-Null
    [IO.Directory]::GetFiles("$cldrFolder/common/main") | % {
        $src = [xml]([IO.File]::ReadAllText($_))
        $node = $src.SelectSingleNode("/ldml/$xpath")
        if ($node) {
            $node = $doc.ImportNode($node, $true)
            $doc.FirstChild.AppendChild($node) | Out-Null
            AddAttribute $node 'locale' ($IntlUtility::CanonicalizeLanguageTag([IO.Path]::GetFileNameWithoutExtension($_)))
        }
    }
    if ($preprocess) {
        Invoke-Command -ScriptBlock $preprocess -InputObject $doc
    }
    $doc.SelectNodes('//alias[@source = "locale"]') | % {
        $path = $_.Attributes.GetNamedItem('path').Value
        $node = $_.ParentNode.SelectSingleNode($path)
        if (-not $node) {
            Write-Host "Unable to resolve $path" -ForegroundColor Red
        } else {
            $nid = $node.Attributes.GetNamedItem('nid')
            if (-not $nid) {
                AddAttribute $node 'nid' "n$count"
                $count = $count + 1
                $nid = $node.Attributes.GetNamedItem('nid')
            }
            AddAttribute $_.ParentNode 'aliasOf' $nid.Value
        }
        RemoveNodes $_
    }
    CanonicalizeLocales $doc
    CanonicalizeExtensionValues $doc
    $doc.SelectNodes('//*[@aliasOf]') | % {
        $nid = $_.Attributes.GetNamedItem('aliasOf').Value
        $node = $doc.SelectSingleNode("//*[@nid = '$nid']")
        $parents = @($_.SelectNodes('ancestor-or-self::*'))
        $diffNode = @($node.SelectNodes('ancestor-or-self::*') | ? { -not $parents.Contains($_) })[0]
        $use = @('type', 'numberSystem') | % { $diffNode.Attributes.GetNamedItem($_).Value } | ? { $_ }
        if (-not $use) {
            $use = $diffNode.Name
        }
        AddAttribute $_ 'use' $use
    }
    $doc.SelectNodes('//*[@nid and @aliasOf]') | % {
        $nid = $_.Attributes.GetNamedItem('nid');
        $sid = $_.Attributes.GetNamedItem('aliasOf');
        $use = $_.Attributes.GetNamedItem('use');
        $doc.SelectNodes("//*[@aliasOf = '$($nid.Value)']") | % {
            $_.Attributes.GetNamedItem('aliasOf').Value = $sid.Value
            $_.Attributes.GetNamedItem('use').Value = $use.Value
        }
        RemoveNodes $nid
    }
    $patterns | % {
        $doc.SelectNodes($_) | % {
            if (AreAllPatternsInherited $_) {
                AddAttribute $_.ParentNode 'inherits' 'true'
                RemoveNodes $_
            } else {
                $inherited = @($_.SelectNodes("*[text() = '$placeholder']"))
                if ($inherited.Count -gt 0) {
                    RemoveNodes $inherited
                    AddAttribute $_ 'inherits' 'true'
                }
            }
        }
    }
    RemoveNodes $doc '//@draft|//@nid|//@aliasOf'
    WriteXmlAndGZ $doc "$projFolder/Data/$filename"
}
      
function ExtractSupplemental($filename, $preprocess) {
    $doc = New-Object Xml.XmlDocument
    $settings = New-Object Xml.XmlReaderSettings
    $settings.IgnoreComments = $true
    $settings.DtdProcessing = [Xml.DtdProcessing]"Ignore"
    $doc.Load([Xml.XmlReader]::Create("$cldrFolder/common/supplemental/$filename", $settings))
    CanonicalizeLocales $doc
    CanonicalizeExtensionValues $doc
    if ($preprocess) {
        Invoke-Command -ScriptBlock $preprocess -InputObject $doc
    }
    WriteXmlAndGZ $doc "$projFolder/Data/$filename"
}

function ExtractBCP() {
    $doc = New-Object Xml.XmlDocument
    $doc.AppendChild($doc.CreateElement('ldmlBCP47')) | Out-Null
    $doc.FirstChild.AppendChild($doc.CreateElement('keyword')) | Out-Null
    [IO.Directory]::GetFiles("$cldrFolder/common/bcp47") | % {
        $src = [xml]([IO.File]::ReadAllText($_))
        $src.SelectNodes("/ldmlBCP47/keyword/key") | % {
            $node = $doc.ImportNode($_, $true)
            $doc.FirstChild.FirstChild.AppendChild($node) | Out-Null
        }
    }
    RemoveNodes $doc "//@description"
    WriteXmlAndGZ $doc "$projFolder/Data/bcp47.xml"
}

ExtractMain 'numbers' 'numbers.xml' ({
    RemoveNodes $Input '//currency/displayName'
    RemoveNodes $Input '//minimalPairs'
    RemoveNodes $Input '//miscPatterns'
    @($Input.SelectNodes('//currency')) | % {
        $sym = $_.SelectSingleNode('symbol[not(@alt)]')
        if ($sym -and $sym.FirstChild.Value -eq $_.Attributes.GetNamedItem('type').Value) {
            RemoveNodes $sym
        }
        if ($_.ChildNodes.Count -eq 0 -or (AreAllPatternsInherited $_)) {
            RemoveNodes $_
        }
    }
})
ExtractMain 'units' 'units.xml' ({
    @($Input.SelectNodes('//unit') | ? {
        $type = $_.Attributes.GetNamedItem("type").Value
        $type = $type.Substring($type.IndexOf('-') + 1)
        -not $IntlUtility::IsWellFormedUnitIdentifier($type)
    }) | % { RemoveNodes $_ } | Out-Null
    RemoveNodes $Input '//compoundUnit[@type != "per"]'
    RemoveNodes $Input '//coordinateUnit'
    RemoveNodes $Input '//displayName'
})
ExtractMain 'dates/fields' 'dateFields.xml' ({
    RemoveNodes $Input '//displayName'
    RemoveNodes $Input '//field[count(*) = 0]'
    RemoveNodes $Input '//field[starts-with(@type, "era")]'
    RemoveNodes $Input '//field[starts-with(@type, "weekOfMonth")]'
    RemoveNodes $Input '//field[starts-with(@type, "dayOfYear")]'
    RemoveNodes $Input '//field[starts-with(@type, "weekdayOfMonth")]'
    RemoveNodes $Input '//field[starts-with(@type, "sun")]'
    RemoveNodes $Input '//field[starts-with(@type, "mon")]'
    RemoveNodes $Input '//field[starts-with(@type, "tue")]'
    RemoveNodes $Input '//field[starts-with(@type, "wed")]'
    RemoveNodes $Input '//field[starts-with(@type, "thu")]'
    RemoveNodes $Input '//field[starts-with(@type, "fri")]'
    RemoveNodes $Input '//field[starts-with(@type, "sat")]'
    RemoveNodes $Input '//field[starts-with(@type, "dayperiod")]'
})
ExtractMain 'dates/calendars' 'calendars.xml' ({
    $Input.SelectNodes('//*[@type = "format"]/*/alias') | % {
        $context = $_.ParentNode
        $path = $_.Attributes.GetNamedItem('path').Value
        if ($path.Contains('stand-alone')) {
            RemoveNodes $_
            $node = $context.SelectSingleNode($path)
            @($node.CloneNode($true).ChildNodes) | % {
                $context.AppendChild($_) | Out-Null
            }
        }
    }
    RemoveNodes $Input '//*[@type = "stand-alone"]'
    RemoveNodes $Input '//quarters'
})
ExtractMain 'listPatterns' 'listPatterns.xml'
ExtractMain 'dates/timeZoneNames' 'timeZoneNames.xml' ({
    RemoveNodes $Input '//zone[descendant::exemplarCity]'
})

ExtractSupplemental 'metaZones.xml'
ExtractSupplemental 'ordinals.xml' ({
    RemoveNodes $Input '//pluralRule[@count = "other"]'
    TransformValues $Input '//pluralRule' ({ $Input -replace '\s*@.+$','' })
})
ExtractSupplemental 'plurals.xml' ({
    RemoveNodes $Input '//pluralRule[@count = "other"]'
    TransformValues $Input '//pluralRule' ({ $Input -replace '\s*@.+$','' })
})
ExtractSupplemental 'likelySubtags.xml' ({
    TransformValues $Input '//likelySubtag/@from' ({ $BcpLanguageTag::Parse($Input).ToString() })
    TransformValues $Input '//likelySubtag/@to' ({ $BcpLanguageTag::Parse($Input).ToString() })
})
ExtractSupplemental 'supplementalData.xml' ({
    RemoveNodes $Input '/supplementalData/references'
    RemoveNodes $Input '/supplementalData/codeMappings'
    RemoveNodes $Input '/supplementalData/measurementData'
    RemoveNodes $Input '/supplementalData/territoryContainment'
})
ExtractSupplemental 'supplementalMetadata.xml' ({
    RemoveNodes $Input '//serialElements'
    TransformValues $Input '//alias/*/@type' ({ $Input -replace '_','-' })
    TransformValues $Input '//alias/*/@replacement' ({ $Input -replace '_','-' })
})

ExtractBCP
