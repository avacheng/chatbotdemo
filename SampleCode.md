## Slide 26
```
private readonly IConfiguration _config;
public EchoBot(IConfiguration config)
{
	_config = config;
}
```

## Slide 27
```
private async Task<string> GetQnAResponse(string question)
{
	using (var client = new HttpClient())
	using (var request = new HttpRequestMessage())
	{
		request.Method = HttpMethod.Post;
		request.RequestUri = new Uri(_config["QnAUri"]);
		request.Content = new StringContent("{question:'" + question + "'}", Encoding.UTF8, "application/json");

		// The value of the header contains the string/text 'EndpointKey ' with the trailing space
		request.Headers.Add("Authorization", "EndpointKey " + _config["QnAEndpointKey"]);

		var response = await client.SendAsync(request);
		var responseBody = await response.Content.ReadAsStringAsync();
		return JObject.Parse(responseBody)["answers"][0]["answer"].ToString();
	}
}
```

## Slide 28
```
var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), _config["MicrosoftAppId"], _config["MicrosoftAppPassword"]);
var reply = (turnContext.Activity as Activity).CreateReply();
string userWords = turnContext.Activity.Text;
string predictionResult;

if (!string.IsNullOrWhiteSpace(userWords))
{
	// Get the answer from the QnA maker
	predictionResult = await GetQnAResponse(userWords);
	if (predictionResult != "No good match found in KB.")
	{
		reply = (turnContext.Activity as Activity).CreateReply(predictionResult);
	}

	if (reply.Text.Length == 0)
	{
		reply.Text = "不好意思，機器人客服無法判斷您的意思，請重新說明您的問題";
	}
	
	await connector.Conversations.ReplyToActivityAsync(reply);
}			
```

## Slide 38
```
if (turnContext.Activity.ChannelId.ToLower() == "line")
{
	isRock.LineBot.Utility.ReplyMessage(reply.ReplyToId, reply.Text, _config["LineAccessToken"]);
}
else
{
	await connector.Conversations.ReplyToActivityAsync(reply);
}
```

## Slide 44
```
private async Task<Dictionary<string, string>> GetLUISPrediction(string text)
{
	using (var client = new HttpClient())
	using (var request = new HttpRequestMessage())
	{
		var queryString = HttpUtility.ParseQueryString(string.Empty);
		// Request headers
		client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config["LuisKey"]);
		// Request parameters
		queryString["verbose"] = "true";
		//queryString["staging"] = "{boolean}";
		var uri = _config["LuisUri"] + queryString;

		HttpResponseMessage response;
		byte[] byteData = Encoding.UTF8.GetBytes("\"" + text + "\"");
		using (var content = new ByteArrayContent(byteData))
		{
			response = await client.PostAsync(uri, content);
			var result = await response.Content.ReadAsStringAsync();
			var jsonObject = JObject.Parse(result);
			string intent = jsonObject["intents"][0]["intent"].ToString();
			string entity = "但是我無法進一步分析" + intent;
			if (jsonObject.ContainsKey("compositeEntities"))
			{
				entity = jsonObject["compositeEntities"][0]["value"].ToString();
			}
			else
			{
				if (jsonObject["entities"].Count() > 0)
				{
					entity = jsonObject["entities"][0]["entity"].ToString();
				}
			}
			return new Dictionary<string, string>()
			{ {"Intent", intent},
			  {"Entity", entity}};
		}
	}
}
```

```
var luisPredction = await GetLUISPrediction(userWords);
if (luisPredction["Intent"] != "None")
{
	predictionResult = "OK，你想要" + luisPredction["Intent"] + "，" + luisPredction["Entity"];
	reply.Text = predictionResult;
}
else
{
	// Get the answer from the QnA maker
	// Todo
}
```

## Slide 48
```
requests
| where url endswith "generateAnswer"
| project timestamp, id, name, resultCode, duration
| parse name with *"/knowledgebases/"KbId"/generateAnswer"
| join kind= inner (
traces | extend id = operation_ParentId
) on id
| extend question = tostring(customDimensions['Question'])
| extend answer = tostring(customDimensions['Answer'])
| project KbId, timestamp, resultCode, duration, question, answer
```

## Slide 49
```
CREATE TABLE ChatRecords (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Channel varchar(50) NOT NULL,
    UserId varchar(255) NOT NULL,
    UserName nvarchar(255),
    UserRequest nvarchar(2000) NOT NULL,
    BotResponse nvarchar(2000) NOT NULL,
    RequestJson nvarchar(4000) NOT NULL,
    Created datetime,
    SurveyAns nvarchar(100),
    Surveyed datetime,
);
```

## Slide 50
```
private void CollectRequestData(ITurnContext<IMessageActivity> turnContext, string answer)
{
	// Convert UTC Time to Taipei Time
	DateTime timeUtc = DateTime.UtcNow;
	TimeZoneInfo taipeiZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
	DateTime taipeiTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, taipeiZone);
	var json = JsonConvert.SerializeObject(turnContext.Activity);

	string cmdText = "INSERT INTO [ChatRecords] ([Channel],[UserId],[UserName],";
	cmdText += "[UserRequest],[BotResponse],[RequestJson],[Created],[SurveyAns],[Surveyed]) ";
	cmdText += "VALUES (@Channel,@UserId,@UserName,@UserRequest,@BotResponse,@RequestJson,@Created,'',@Created);";

	using (SqlCommand cmd = new SqlCommand())
	{
		cmd.CommandText = cmdText;
		cmd.CommandType = CommandType.Text;
		cmd.Parameters.Add("@Channel", SqlDbType.VarChar).Value = turnContext.Activity.ChannelId;
		cmd.Parameters.Add("@UserId", SqlDbType.VarChar).Value = turnContext.Activity.From.Id;
		cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = turnContext.Activity.From.Name;
		cmd.Parameters.Add("@UserRequest", SqlDbType.NVarChar).Value = turnContext.Activity.Text;
		cmd.Parameters.Add("@BotResponse", SqlDbType.NVarChar).Value = answer;
		cmd.Parameters.Add("@RequestJson", SqlDbType.NVarChar).Value = json;
		cmd.Parameters.Add("@Created", SqlDbType.DateTime).Value = taipeiTime;
		using (SqlConnection con = new SqlConnection(_config["SqlConnStr"]))
		{
			cmd.Connection = con;
			con.Open();
			cmd.ExecuteNonQuery();
		}
	}
}
```

## Slide 53 + 54
```
if (turnContext.Activity.ChannelId.ToLower() == "line")
{
	// LINE ButtonsTemplate 有字數限制
	// LINE Templates 手機上無法顯示
	string puretext = System.Text.RegularExpressions.Regex.Replace(reply.Text, "<.*?>", string.Empty);
	if (createButtons && puretext.Length <= 50)
	{
		var ButtonsTemplateMsg = new isRock.LineBot.ButtonsTemplate();
		ButtonsTemplateMsg.text = puretext + "請問有幫助到您嗎?";
		ButtonsTemplateMsg.title = "查詢回覆";
		var actions = new List<isRock.LineBot.TemplateActionBase>();
		actions.Add(new isRock.LineBot.MessageAction() { label = "<<很有用>>", text = "<<很有用>>" });
		actions.Add(new isRock.LineBot.MessageAction() { label = "<<普通>>", text = "<<普通>>" });
		actions.Add(new isRock.LineBot.MessageAction() { label = "<<再加強>>", text = "<<再加強>>" });
		ButtonsTemplateMsg.actions = actions;
		isRock.LineBot.Utility.ReplyTemplateMessage(reply.ReplyToId, ButtonsTemplateMsg, _config["LineAccessToken"]);
	}
	else
	{
		isRock.LineBot.Utility.ReplyMessage(reply.ReplyToId, reply.Text, _config["LineAccessToken"]);
	}
}
else
{
	if (createButtons)
	{
		reply = (turnContext.Activity as Activity).CreateReply(reply.Text + "\n\n請問有幫助到您嗎?");
		reply.SuggestedActions = new SuggestedActions()
		{
			Actions = new List<CardAction>()
			{
				new CardAction() { Title = "<<很有用>>", Type = ActionTypes.ImBack, Value = "<<很有用>>" },
				new CardAction() { Title = "<<普通>>", Type = ActionTypes.ImBack, Value = "<<普通>>" },
				new CardAction() { Title = "<<再加強>>", Type = ActionTypes.ImBack, Value = "<<再加強>>" }
			},
		};
	}
	await connector.Conversations.ReplyToActivityAsync(reply);
}
```

## Slide 55
```
private void CollectSurveyData(ITurnContext<IMessageActivity> turnContext, string result)
{
	DateTime timeUtc = DateTime.UtcNow;
	TimeZoneInfo taipeiZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
	DateTime taipeiTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, taipeiZone);

	string cmdText = "WITH temp AS (SELECT MAX(Id)maxid FROM ChatRecords WHERE Channel=@Channel AND UserId=@UserId)";
	cmdText += "UPDATE ChatRecords SET SurveyAns=@SurveyAns, Surveyed=@Surveyed ";
	cmdText += "FROM ChatRecords INNER JOIN temp ON Id = maxid WHERE SurveyAns='';";

	using (SqlCommand cmd = new SqlCommand())
	{
		cmd.CommandText = cmdText;
		cmd.CommandType = CommandType.Text;
		cmd.Parameters.Add("@Channel", SqlDbType.VarChar).Value = turnContext.Activity.ChannelId;
		cmd.Parameters.Add("@UserId", SqlDbType.VarChar).Value = turnContext.Activity.From.Id;
		cmd.Parameters.Add("@SurveyAns", SqlDbType.NVarChar).Value = gradeDictionary.ContainsKey(result) ? gradeDictionary[result] : result;
		cmd.Parameters.Add("@Surveyed", SqlDbType.DateTime).Value = taipeiTime;
		using (SqlConnection con = new SqlConnection(_config["SqlConnStr"]))
		{
			cmd.Connection = con;
			con.Open();
			cmd.ExecuteNonQuery();
		}
	}
}
```

## Slide 56
```
if (userWords.StartsWith("<<") && userWords.EndsWith(">>"))
{
	reply.Text = "謝謝您！歡迎繼續發問喔！";
	answerServey = true;
}
```

```
if (answerServey)
{
	CollectSurveyData(turnContext, userWords);
}
else
{
	CollectRequestData(turnContext, predictionResult);
}
```

