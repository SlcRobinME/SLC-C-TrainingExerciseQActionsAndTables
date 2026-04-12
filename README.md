# 1. Training Exercise QActions & Tables

## Description

The goal of this exercise is to parse a JSON file and fill in the data in the corresponding tables.
The JSON file contains an overview of transportstreams and services, the file is available in the Documentation folder of this repo.

* Create a Transportstreams table.
* Create a Services table.
* The columns needed are the properties defined in the JSON file.
* Add an extra column which reflects the last time the row was polled (DateTime).
* The data should be polled every minute.
* A button should be available to force poll the data.

## Pointers

Handle this exercise as would be for a customer.

* All parameter names/descriptions should be carefully chosen to really reflect the value.
* No errors/major/minor/... should be reported by the DIS validator.
* No magic numbers in QActions.
* Some rules when working with tables:
  * Every column name should start with the table name.
  * Every column description has the table description between round brackets.

## Tips

* You can either hardcode the JSON in your QAction or read the file from your file system (-> "C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\Data.json")
* To parse the JSON use NewtonSoft.Json.
* Consider using the QActionTableRows for updating tables.

---
---

# 2. Training Exercise: Unit Testing

## Description

By utilizing the [UnitTestingFramework](https://github.com/SkylineCommunications/Skyline.DataMiner.Utils.UnitTestingFramework/pkgs/nuget/Skyline.DataMiner.Utils.UnitTestingFramework.Protocol), make sure the following parts are tested and verified correctly:
1. Parsing of the JSON data
2. Populating the table data & assert that the added row(s) contain the correct values.

## Requirement
* Complete section *2.4 Unit Test Training* from the training document first ([Videos](https://community.dataminer.services/courses/data-ingest-control-plane-development-unit-testing/lessons/unit-testing-introduced/))

---
---

# 3. Training Exercise: TreeControl

## Description

Since the data is structured in a relation way, the end user is requesting a better visualization than the default tables and would like to see the data structured in a Tree View.
Based on the JSON data, define a logical structure for the tree view and apply the layout.

## Requirements

* The data is structured in a tree view, [example](https://docs.dataminer.services/develop/images/uiX_-_advanced_hierarchy.png).
* Alarms bubble up to the top level correctly, similar as the View structure behaves within Cube.