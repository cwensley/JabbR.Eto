#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

function extract_csproj () {
	csprojdir=$1
	csproj=$2
	subdir="`echo $csprojdir | sed 's/\\//\\\\\\\\/g'`"
	subdir="$subdir\\\\"
	
	
	rm -f "$DIR/$csproj"

	cp "$DIR/$csprojdir/$csproj" "$csproj"

	#rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\">(?=\s*<Link>)/<\1 Include=\"$subdir\2\">/g"
	rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\">(?!\s*<Link>)/<\1 Include=\"$subdir\2\">/g"
	perl -0pi -e "$rep" "$csproj"
	
	#rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\">(?!\s*<Link>)/<\1 Include=\"$subdir\2\">\n      <Link>\2<\/Link>/g"
	rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\">\s*<Link>.+<\/Link>/<\1 Include=\"$subdir\2\">/g"
	perl -0pi -e "$rep" "$csproj"
	
	#rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\" \/>/<\1 Include=\"$subdir\2\">\n      <Link>\2<\/Link>\n    <\/\1>/"
	rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\" \/>/<\1 Include=\"$subdir\2\" \/>/"
	perl -pi -e "$rep" "$csproj"
	
	rep="s/<(OutputPath|DocumentationFile)>(.+)<\//<\1>$subdir\2<\//"
	perl -pi -e "$rep" "$csproj"
	
	rep="s/(?<=<Import Project=\")(?!\$\(.*\)))(.+)(?=\"\s*\/>)/$subdir\2/g"
	perl -0pi -e "$rep" "$csproj"
	
	rep="s/(<Reference Include=\"[^\"]++\">(\s|.)*?<HintPath>)((\s|.)+?<\/HintPath>(\s|.)+?<\/Reference>)/\1$subdir\3/g"
	perl -0pi -e "$rep" "$csproj"
}

function replace_reference () {
	csproj=$1
	find_reference=$2
	replace_with=$3
	
	uuid=$(sed -n 's/.*<ProjectGuid>\(.*\)<\/ProjectGuid>.*/\1/p' "$csproj")
	
	rep="s/<Reference Include=\"$find_reference.*?\">(\s|.)*?<\/Reference>/<ProjectReference Include=\"$replace_with.csproj\">\n      <Project>$uuid<\/Project>\n      <Name>$replace_with<\/Name>\n    <\/ProjectReference>/g"
	perl -0pi -e "$rep" "$csproj"
}

function replace_hint () {
	csprojdir=$1
	find_reference=$2
	newdir=$3

	rep="s/(<Reference Include=\"$find_reference[^\"]++\">(\s|.)*?<HintPath>)([^<]+?)($find_reference[.]dll<\/HintPath>(\s|.)+?<\/Reference>)/\1$newdir\4/g"
	perl -0pi -e "$rep" "$csproj"
}

function remove_line () {
	file_name=$1
	remove_line=$2
	
	perl -0pi -e "$remove_line" "$file_name"
}


extract_csproj "SignalR/src/Microsoft.AspNet.SignalR.Client" "Microsoft.AspNet.SignalR.Client.csproj"
replace_reference "Microsoft.AspNet.SignalR.Client.csproj" "Newtonsoft.Json" "Newtonsoft.Json"
remove_line "Microsoft.AspNet.SignalR.Client.csproj" "s/.*<Import Project=\"\\$\\(SolutionDir\\)\.nuget\\\\NuGet\.targets\" \/>.*\n//g"

extract_csproj "Newtonsoft.Json/Src/Newtonsoft.Json" "Newtonsoft.Json.csproj"
remove_line "Newtonsoft.Json.csproj" "s/.*<TargetFrameworkProfile>Client<\\/TargetFrameworkProfile>.*\n//g"

extract_csproj "JabbR.Client/JabbR.Client" "JabbR.Client.csproj"
replace_reference "JabbR.Client.csproj" "Newtonsoft.Json" "Newtonsoft.Json"
replace_reference "JabbR.Client.csproj" "Microsoft.AspNet.SignalR.Client" "Microsoft.AspNet.SignalR.Client"
remove_line "JabbR.Client.csproj" "s/.*<Import Project=\"\\$\\(SolutionDir\\)\\\\\.nuget\\\\nuget\.targets\" \/>.*\n//g"
