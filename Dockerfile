FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY bin/Release/net6.0/ ./
VOLUME /contents
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TDK-Boilerplate-C#.dll"]