// Copyright 2017 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SimpleWebSampleV2.Controllers
{
    // This controller lists configuration settings and their value.
    // Please, do try to update the appsettings.json while the application is running. 😉

    [Route("api/[controller]")]
    public class ConfigurationController : Controller
    {
        public ConfigurationController(IConfiguration configuration, ILogger<ConfigurationController> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public ILogger Logger { get; }

        [HttpGet]
        public IEnumerable<KeyValuePair<string, string>> Get()
        {
            Logger.LogInformation("Listing all configuration settings…");

            return Configuration.AsEnumerable();
        }

        [HttpGet("{key}")]
        public string Get(string key)
        {
            Logger.LogInformation("The configuration key {ConfigurationKey} was requested.", key);
            string value = Configuration[key];
            Logger.LogInformation("The configuration key {ConfigurationKey} has value {ConfigurationValue}.", key, value);

            return value;
        }
    }
}
