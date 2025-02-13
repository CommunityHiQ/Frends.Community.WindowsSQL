 # Frends.Community.WindowsSQL

Frends Community SQL Tasks for Windows machines with simple impersonation.

 [![Actions Status](https://github.com/CommunityHiQ/Frends.Community.WindowsSQL/workflows/PackAndPushAfterMerge/badge.svg)](https://github.com/CommunityHiQ/Frends.Community.WindowsSQL/actions) ![MyGet](https://img.shields.io/myget/frends-community/v/Frends.Community.WindowsSQL) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) 
 
 - [Frends.Community.WindowsSQL](#frends.community.windowssql)
   - [Installing](#installing)
   - [Building](#building)
   - [Contributing](#contributing)
   - [Documentation](#documentation)
     - [Sql.ExecuteQuery](#sqlexecutequery) 
     - [Sql.ExecuteProcedure](#sqlexecuteprocedure) 
     - [Sql.BulkInsert](#sqlbulkinsert)
     - [Sql.BatchOperation](#sqlbatchoperation) 
   - [License](#license)

## Installing
You can install the task via FRENDS UI Task view, by searching for packages. You can also download the latest NuGet package from https://www.myget.org/feed/frends/package/nuget/Frends.Community.WindowsSQL and import it manually via the Task view.

## Building
Clone a copy of the repo

`git clone https://github.com/CommunityHiQ/Frends.Community.WindowsSQL.git`

Restore dependencies

`dotnet restore`

Rebuild the project

`dotnet build`

Run Tests
To run the tests you will need an SQL server. You can set the database connection string in test project [appsettings.json](Frends.Community.WindowsSQL.Tests/appsettings.json) file

`dotnet test Frends.Community.WindowsSQL.Tests`

Create a nuget package

`dotnet pack Frends.Community.WindowsSQL`

## Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repo on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!

## Documentation

### WindowsSQL.ExecuteQuery
#### Input 
| Property          | Type                              | Description                                             | Example                                   |
|-------------------|-----------------------------------|---------------------------------------------------------|-------------------------------------------|
| Query             | string                            | The query that will be executed to the database.        | `select Name,Age from MyTable where AGE = @Age` 
| Parameters        | Array{Name: string, Value: string} | A array of parameters to be appended to the query.     | `Name = Age, Value = 42`
| Connection String | string                            | Connection String to be used to connect to the database.| `Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;`


#### Options
| Property               | Type                 | Description                                                |
|------------------------|----------------------|------------------------------------------------------------|
| Command Timeout        | int                  | Timeout in seconds to be used for the query. 60 seconds by default. |
| Sql Transaction Isolation Level | SqlTransationIsolationLevel | Transactions specify an isolation level that defines the degree to which one transaction must be isolated from resource or data modifications made by other transactions. Possible values are: Default, None, Serializable, ReadUncommitted, ReadCommitted, RepeatableRead, Snapshot. Additional documentation https://msdn.microsoft.com/en-us/library/ms378149(v=sql.110).aspx |
| UseGivenUserCredentialsForRemoteConnections   | bool | Allows you to give the user credentials to use to make sql executions on remote hosts. |
| UserName               | string               | Username for simple impersonation. Needs to be of format domain\username |
| Password               | string               | Password for the given credentials.                        |

#### Result
JToken. JObject[]

Example result
```
[ 
 {
  "Name": "Foo",
  "Age": 42
 },
 {
  "Name": "Adam",
  "Age": 42
 }
]
```
```
The second name 'Adam' can be now be accessed by #result[1].Name in the process parameter editor.

```


### WindowsSQL.ExecuteProcedure
#### Input
| Property          | Type                              | Description                                             | Example                                   |
|-------------------|-----------------------------------|---------------------------------------------------------|-------------------------------------------|
| Execute           | string                            | The stored procedure that will be executed.             | `SpGetResultsByAge` 
| Parameters        | Array{Name: string, Value: string} | A array of parameters to be appended to the query.     | `Name = Age, Value = 42`
| Connection String | string                            | Connection String to be used to connect to the database.| `Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;`

#### Options
| Property               | Type                 | Description                                                |
|------------------------|----------------------|------------------------------------------------------------|
| Command Timeout        | int                  | Timeout in seconds to be used for the query. 60 seconds by default. |
| Sql Transaction Isolation Level | SqlTransationIsolationLevel | Transactions specify an isolation level that defines the degree to which one transaction must be isolated from resource or data modifications made by other transactions. Possible values are: Default, None, Serializable, ReadUncommitted, ReadCommitted, RepeatableRead, Snapshot. Additional documentation https://msdn.microsoft.com/en-us/library/ms378149(v=sql.110).aspx |
| UseGivenUserCredentialsForRemoteConnections   | bool | Allows you to give the user credentials to use to make sql executions on remote hosts. |
| UserName               | string               | Username for simple impersonation. Needs to be of format domain\username |
| Password               | string               | Password for the given credentials.                        |

#### Result
JToken. JObject[]

Example result
```
[ 
 {
  "Name": "Foo",
  "Age": 42
 },
 {
  "Name": "Adam",
  "Age": 42
 }
]
```
```
The second name 'Adam' can be now be accessed by #result[1].Name in the process parameter editor.

```

### WindowsSQL.BulkInsert
#### Input
| Property          | Type   | Description                                                                                                                                                                                                                                      | Example                                                                            |
|-------------------|--------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| Input Data        | string | The data that will be inserted into the database. The data is a json string formated as Json Array of objects. The data has to have the same number of columns in the same order as the destination table.                                                   | `[{"Column1": "One", "Column2": 10},{"Column1": "Two", "Column2": 20}]`         |
| Table Name        | string | Destination table name.                                                                                                                                                                                                                          | MyTable                                                                            |
| Connection String | string | Connection String to be used to connect to the database.                                                                                                                                                                                         | Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword; |

 
#### Options
| Property                         | Type                        | Description                                                                                                                                                                                                                                                                                                                                                       |
|----------------------------------|-----------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Command Timeout Seconds          | int                         | Timeout in seconds to be used for the query. Default is 60 seconds,                                                                                                                                                                                                                                                                                               |
| Fire Triggers                    | bool                        | When specified, cause the server to fire the insert triggers for the rows being inserted into the database.                                                                                                                                                                                                                                                       |
| Keep Identity                    | bool                        | Preserve source identity values. When not specified, identity values are assigned by the destination.                                                                                                                                                                                                                                                             |
| Sql Transaction Isolation Level  | SqlTransationIsolationLevel | Transactions specify an isolation level that defines the degree to which one transaction must be isolated from resource or data modifications made by other transactions. Possible values are: Default, None, Serializable, ReadUncommitted, ReadCommitted, RepeatableRead, Snapshot. Additional documentation https://msdn.microsoft.com/en-us/library/ms378149(v=sql.110).aspx |
| Convert Empty PropertyValues To Null | bool                    | If the input properties have empty values i.e. "", the values will be converted to null if this parameter is set to true.                                                                                                                                                                                                                                                     |
| UseGivenUserCredentialsForRemoteConnections   | bool | Allows you to give the user credentials to use to make sql executions on remote hosts. |
| UserName               | string               | Username for simple impersonation. Needs to be of format domain\username |
| Password               | string               | Password for the given credentials.                        |

#### Result
Integer - Number of copied rows

### WindowsSQL.BatchOperation
#### Input
| Property          | Type                              | Description                                             | Example                                   |
|-------------------|-----------------------------------|---------------------------------------------------------|-------------------------------------------|
| Query             | string                            | The query that will be executed to the database.        | `insert into MyTable(ID,NAME) VALUES (@Id, @FirstName)` 
| Input Json        | string                            | A Json Array of objects that has their properties mapped to the parameters in the Query      | `[{"Id":10, "FirstName": "Foo"},{"Id":15, "FirstName": "Bar"}]`
| Connection String | string                            | Connection String to be used to connect to the database.| `Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;`


#### Options
| Property               | Type                 | Description                                                |
|------------------------|----------------------|------------------------------------------------------------|
| Command Timeout Seconds       | int                  | Timeout in seconds to be used for the query. 60 seconds by default. |
| Sql Transaction Isolation Level | SqlTransationIsolationLevel | Transactions specify an isolation level that defines the degree to which one transaction must be isolated from resource or data modifications made by other transactions. Possible values are: Default, None, Serializable, ReadUncommitted, ReadCommitted, RepeatableRead, Snapshot. Additional documentation https://msdn.microsoft.com/en-us/library/ms378149(v=sql.110).aspx | 
| UseGivenUserCredentialsForRemoteConnections   | bool | Allows you to give the user credentials to use to make sql executions on remote hosts. |
| UserName               | string               | Username for simple impersonation. Needs to be of format domain\username |
| Password               | string               | Password for the given credentials.                        |

#### Result
Integer - Number of affected rows

#### Example usage
![BatchOperationExample.png](https://cloud.githubusercontent.com/assets/6636662/26483905/3bb0d73c-41f8-11e7-95c9-fe554898f97f.png)

## License

This project is licensed under the MIT License - see the LICENSE file for details

# Change Log

| Version | Changes |
| ------- | ------- |
| 1.0.0   | Initial implementation |
| 1.1.0   | Added Simple impersonation to BatchOperation and BulkInsert methods |
| 2.0.0   | Targeted to .NET6 and 8 and updated the following packages: Dapper to 2.1.35, System.Data.SqlClient to Microsoft.Data.SqlClient 5.2.2, System.ComponentModel.Annotations to 5.0.0 and SimpleImpersonation to 4.2.0 |