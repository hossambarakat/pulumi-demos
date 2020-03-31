using System.Collections.Generic;
using System.Threading.Tasks;

using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Azure.CosmosDB.Inputs;
using TeamTimeZonesInfrastructure;
using Pulumi.Azure.CosmosDB;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Shared.Protocol;
using System;
using TeamTimeZonesInfrastructure.Complete;
using TeamTimeZonesInfrastructure.Step1;
using TeamTimeZonesInfrastructure.Step2;
using TeamTimeZonesInfrastructure.Step3;

class Program
{
    static Task<int> Main()
    {
        return Deployment.RunAsync<Step1Start>();
    }

}
