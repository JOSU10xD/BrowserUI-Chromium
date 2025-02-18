using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserUIMultiCore;

public class User
{
    public Guid UserId { get; set; }

    public string Username { get; set; }

    [JsonProperty("UserSettings")]

    public Settings UserSettings { get; set; }
 }
