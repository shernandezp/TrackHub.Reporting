﻿// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using System.Globalization;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Interfaces.Router;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory;

public class PositionRecord(IRouterReader reader, IExcelHelper helper) : IReport
{
    public string ReportCode => Reports.PositionRecord;
    public async Task<byte[]> GenerateAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var data = await reader.GetPositionsRecordAsync(filters, cancellationToken);
        var culture = new CultureInfo(filters.Language);
        return helper.Export(filters.Name, filters.DateTimeFilter1, filters.DateTimeFilter2, data, culture);
    }
}
