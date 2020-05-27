using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Pulumi.Aws.Glue;
using Pulumi.Aws.Glue.Inputs;

namespace AwsMeetupGroup.DataServices.Infrastructure {
     static class Glue{

            public static CatalogDatabase CreateGlueDatabase(string databaseName) {
                return  new CatalogDatabase(databaseName);
            }         
            
            public static CatalogTable CreateGlueTable(Output<string> databaseName, string tableName, List<string> fields) {
                return new CatalogTable(tableName, new CatalogTableArgs{
                    DatabaseName = databaseName,
                    
                    TableType = "EXTERNAL_TABLE",

                    Parameters = new InputMap<string> {
                        {"EXTERNAL", "TRUE"},
                        {"parquet.compression", "SNAPPY"}
                    },
                    StorageDescriptor = new CatalogTableStorageDescriptorArgs() {
                        Columns = fields.Select(x => new CatalogTableStorageDescriptorColumnArgs() { Name = x, Type = "string" }).ToList(),
                        InputFormat = "json",
                        OutputFormat = "parquet"
                    }                    
                });
            }
     }
}