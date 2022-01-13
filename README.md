# File Share .Net Client

The File Share Service is the single external place for external people to download files from the UKHO. This provides a .Net client to access the file share service.

## Versioning and breaking changes

The File Share dotnet client major version numbers are defined in azure-pipelines.yml 

  - name: UKHOAssemblyVersionPrefix
    value: "1.0."
	
For version 1.0 and beyond we are going to use Semantic Versioning so please only bump the major version on breaking changes.

Moreover breaking changes should be avoided if possible. Only introduce breaking changes after careful consideration. 