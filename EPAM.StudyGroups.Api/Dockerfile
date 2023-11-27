#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY . .
RUN dotnet restore "./EPAM.StudyGroups.Api/EPAM.StudyGroups.Api.csproj"
RUN dotnet build "./EPAM.StudyGroups.Api/EPAM.StudyGroups.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./EPAM.StudyGroups.Api/EPAM.StudyGroups.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

RUN dotnet tool install --version 6.0.13 -g dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef database update --project "./EPAM.StudyGroups.Api/EPAM.StudyGroups.Api.csproj"  --context StudyGroupsContext; 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EPAM.StudyGroups.Api.dll"]