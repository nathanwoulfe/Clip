## clean up from previous runs
rm -r -force nupkgs
rm -r -force ./src/Clip.Backoffice/App_Plugins
mkdir nupkgs

## install backoffice dependencies
cd ./src/Clip.Backoffice
## npm install
npm run prod
cd ../../

## pack individually to avoid clip site blowing up
dotnet pack ./src/Clip.Web/Clip.Web.csproj -c Release -o nupkgs
dotnet pack ./src/Clip.Backoffice/Clip.Backoffice.csproj -c Release -o nupkgs

## pack the container 
dotnet pack ./src/Clip/Clip.csproj -c Release -o nupkgs