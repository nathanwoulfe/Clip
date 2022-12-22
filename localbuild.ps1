## clean up from previous runs
rm -r -force nupkgs
mkdir nupkgs

dotnet restore Clip.sln --locked-mode
dotnet build Clip.sln --configuration Release --no-restore
dotnet pack Clip.sln --configuration Release --no-build --output nupkgs
