#!/bin/bash

set -u
set -e
set -x

version_file="cmdr.Editor/Properties/AssemblyInfo.cs"

version_line="$( cat "$version_file" | grep AssemblyFileVersion )"
version_number="$( echo "$version_line" | awk -F\" '{print $2}' | awk -F\. '{ORS="."; printf("%s.%s.%s", $1,$2, $3)}' )"

tag="cmdr-${version_number}"
final_file="cmdr_tsi_editor_latest.zip"

echo "Going to build version ${version_number}, taken from '${version_file}'. Press CTRL+C to abort."
read 

cd "cmdr.Editor/bin"

rsync -av --progress  --delete "Debug/" "$tag"
 
 
rm "${tag}/Layout.xml"       || true
rm "${tag}/cmdr.exe.Config"  || true

rm -f "$final_file"
zip -r "$final_file" "$tag"
 
mv "$final_file" "../.." 
 
echo "generated: $final_file"

exit 0

 