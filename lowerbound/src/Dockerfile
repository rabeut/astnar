FROM mcr.microsoft.com/dotnet/aspnet:5.0

COPY bin/release/net5.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "LowerBound.dll"]