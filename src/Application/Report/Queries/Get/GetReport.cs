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

using Common.Application.Attributes;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Queries.Get;

[Authorize(Resource = Resources.Reports, Action = Actions.Read)]
public readonly record struct GetReportQuery(string ReportCode, FilterDto Filters) : IRequest<byte[]>;

public class GetReportQueryHandler(IReportFactory factory)
        : IRequestHandler<GetReportQuery, byte[]>
{

    /// <summary>
    /// Handle the GetReportQuery
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        /// Get the report implementation from the factory based on the report id
        var report = factory.GetReport(request.ReportCode);
        return await report.GenerateAsync(request.Filters, cancellationToken);
    }

}
