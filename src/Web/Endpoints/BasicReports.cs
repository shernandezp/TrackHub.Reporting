// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using TrackHub.Reporting.Application.Report.Queries.Get;
using TrackHub.Reporting.Domain.Exceptions;

namespace TrackHub.Reporting.Web.Endpoints;

public class BasicReports : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(GetReport);
    }

    public async Task<IResult> GetReport(ISender sender, GetReportQuery query)
    {
        byte[] fileContent;
        try
        {
            fileContent = await sender.Send(query);
        }
        catch (FeatureDisabledException ex)
        {
            return Results.Json(
                new
                {
                    errors = new[]
                    {
                        new
                        {
                            message = ex.Message,
                            extensions = new { code = ex.Code, featureKey = ex.FeatureKey }
                        }
                    }
                },
                statusCode: StatusCodes.Status403Forbidden);
        }
        catch (ReportLimitExceededException ex)
        {
            return Results.Json(
                new
                {
                    errors = new[]
                    {
                        new
                        {
                            message = ex.Message,
                            extensions = new { code = ex.Code, maxRows = ex.MaxRows }
                        }
                    }
                },
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.File(
            fileContent,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}

