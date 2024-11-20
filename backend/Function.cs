using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

public class Function
{
    private static readonly string TableName = "PageViewCounter";
    private static readonly AmazonDynamoDBClient DynamoDBClient = new AmazonDynamoDBClient();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var getItemRequest = new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue> { { "Id", new AttributeValue { S = "counter" } } }
        };

        var getItemResponse = await DynamoDBClient.GetItemAsync(getItemRequest);
        int count = int.Parse(getItemResponse.Item["Count"].N);

        count += 1; // Increment counter

        var updateItemRequest = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue> { { "Id", new AttributeValue { S = "counter" } } },
            AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
            {
                { "Count", new AttributeValueUpdate { Value = new AttributeValue { N = count.ToString() }, Action = "PUT" } }
            }
        };

        await DynamoDBClient.UpdateItemAsync(updateItemRequest);

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = count.ToString(),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
