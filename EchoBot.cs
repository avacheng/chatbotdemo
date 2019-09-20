// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace �M�צW��.Bots
{
    public class EchoBot : ActivityHandler
    {
        //private readonly IConfiguration _config;
        //public EchoBot(IConfiguration config)
        //{
        //    _config = config;
        //}

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), _config["MicrosoftAppId"], _config["MicrosoftAppPassword"]);
            //var reply = (turnContext.Activity as Activity).CreateReply();
            //string userWords = turnContext.Activity.Text;
            //string predictionResult = "";
            //bool createButtons = false;
            //bool answerSurvey = false;

            //if (!string.IsNullOrWhiteSpace(userWords))
            //{
            //    if (userWords.StartsWith("<<"))
            //    {
            //        reply.Text = "���±z�I�w���~��o�ݳ�I";
            //        answerSurvey = true;
            //    }
            //    else
            //    {
            //        var luisPredction = await GetLUISPrediction(userWords);
            //        if (luisPredction["Intent"] != "None")
            //        {
            //            predictionResult = "OK�A�A�Q�n" + luisPredction["Intent"] + "�A" + luisPredction["Entity"];
            //            reply.Text = predictionResult;
            //        }
            //        else
            //        {
            //            // Get the answer from the QnA maker
            //            predictionResult = await GetQnAResponse(userWords);
            //            if (predictionResult != "No good match found in KB.")
            //            {
            //                reply = (turnContext.Activity as Activity).CreateReply(predictionResult);
            //                createButtons = true;
            //            }

            //        }

            //    }

            //    if (reply.Text.Length == 0)
            //    {
            //        reply.Text = "���n�N��A�����H�ȪA�L�k�P�_�z���N��A�Э��s�����z�����D";
            //    }

            //    if (turnContext.Activity.ChannelId.ToLower() == "line")
            //    {
            //        // LINE ButtonsTemplate ���r�ƭ���
            //        // LINE Templates ����W�L�k���
            //        string puretext = System.Text.RegularExpressions.Regex.Replace(reply.Text, "<.*?>", string.Empty);
            //        if (createButtons && puretext.Length <= 50)
            //        {
            //            var ButtonsTemplateMsg = new isRock.LineBot.ButtonsTemplate();
            //            ButtonsTemplateMsg.text = puretext + "�аݦ����U��z��?";
            //            ButtonsTemplateMsg.title = "�d�ߦ^��";
            //            var actions = new List<isRock.LineBot.TemplateActionBase>();
            //            actions.Add(new isRock.LineBot.MessageAction() { label = "<<�ܦ���>>", text = "<<�ܦ���>>" });
            //            actions.Add(new isRock.LineBot.MessageAction() { label = "<<���q>>", text = "<<���q>>" });
            //            actions.Add(new isRock.LineBot.MessageAction() { label = "<<�A�[�j>>", text = "<<�A�[�j>>" });
            //            ButtonsTemplateMsg.actions = actions;
            //            isRock.LineBot.Utility.ReplyTemplateMessage(reply.ReplyToId, ButtonsTemplateMsg, _config["LineAccessToken"]);
            //        }
            //        else
            //        {
            //            isRock.LineBot.Utility.ReplyMessage(reply.ReplyToId, reply.Text, _config["LineAccessToken"]);
            //        }
            //    }
            //    else
            //    {
            //        if (createButtons)
            //        {
            //            reply = (turnContext.Activity as Activity).CreateReply(reply.Text + "\n\n�аݦ����U��z��?");
            //            reply.SuggestedActions = new SuggestedActions()
            //            {
            //                Actions = new List<CardAction>()
            //                {
            //                    new CardAction() { Title = "<<�ܦ���>>", Type = ActionTypes.ImBack, Value = "<<�ܦ���>>" },
            //                    new CardAction() { Title = "<<���q>>", Type = ActionTypes.ImBack, Value = "<<���q>>" },
            //                    new CardAction() { Title = "<<�A�[�j>>", Type = ActionTypes.ImBack, Value = "<<�A�[�j>>" }
            //                },
            //            };
            //        }
            //        await connector.Conversations.ReplyToActivityAsync(reply);
            //    }
            //}

            //if (answerSurvey)
            //{
            //    CollectSurveyData(turnContext, userWords);
            //}
            //else
            //{
            //    CollectRequestData(turnContext, predictionResult);
            //}
            await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }

        //private async Task<string> GetQnAResponse(string question)
        //{
        //    using (var client = new HttpClient())
        //    using (var request = new HttpRequestMessage())
        //    {
        //        request.Method = HttpMethod.Post;
        //        request.RequestUri = new Uri(_config["QnAUri"]);
        //        request.Content = new StringContent("{question:'" + question + "'}", Encoding.UTF8, "application/json");

        //        // The value of the header contains the string/text 'EndpointKey ' with the trailing space
        //        request.Headers.Add("Authorization", "EndpointKey " + _config["QnAEndpointKey"]);

        //        var response = await client.SendAsync(request);
        //        var responseBody = await response.Content.ReadAsStringAsync();
        //        return JObject.Parse(responseBody)["answers"][0]["answer"].ToString();
        //    }
        //}

        //private void SaveData(string address)
        //{
        //    // Create WebRequest and set request uri string
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
        //    // Http verb for the request
        //    request.Method = "POST";
        //    // Send request and get response
        //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //    // Close the response for new connections
        //    response.Close();
        //}

        //private async Task<Dictionary<string, string>> GetLUISPrediction(string text)
        //{
        //    using (var client = new HttpClient())
        //    using (var request = new HttpRequestMessage())
        //    {
        //        var queryString = HttpUtility.ParseQueryString(string.Empty);
        //        // Request headers
        //        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config["LuisKey"]);
        //        // Request parameters
        //        queryString["verbose"] = "true";
        //        //queryString["staging"] = "{boolean}";
        //        var uri = _config["LuisUri"] + queryString;

        //        HttpResponseMessage response;
        //        byte[] byteData = Encoding.UTF8.GetBytes("\"" + text + "\"");
        //        using (var content = new ByteArrayContent(byteData))
        //        {
        //            response = await client.PostAsync(uri, content);
        //            var result = await response.Content.ReadAsStringAsync();
        //            var jsonObject = JObject.Parse(result);
        //            string intent = jsonObject["intents"][0]["intent"].ToString();
        //            string entity = "���O�ڵL�k�i�@�B���R" + intent;
        //            if (jsonObject.ContainsKey("compositeEntities"))
        //            {
        //                entity = jsonObject["compositeEntities"][0]["value"].ToString();
        //            }
        //            else
        //            {
        //                if (jsonObject["entities"].Count() > 0)
        //                {
        //                    entity = jsonObject["entities"][0]["entity"].ToString();
        //                }
        //            }
        //            return new Dictionary<string, string>()
        //    { {"Intent", intent},
        //      {"Entity", entity}};
        //        }
        //    }
        //}

        //private void CollectRequestData(ITurnContext<IMessageActivity> turnContext, string answer)
        //{
        //    // Convert UTC Time to Taipei Time
        //    DateTime timeUtc = DateTime.UtcNow;
        //    TimeZoneInfo taipeiZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
        //    DateTime taipeiTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, taipeiZone);
        //    var json = JsonConvert.SerializeObject(turnContext.Activity);

        //    string cmdText = "INSERT INTO [ChatRecords] ([Channel],[UserId],[UserName],";
        //    cmdText += "[UserRequest],[BotResponse],[RequestJson],[Created],[SurveyAns],[Surveyed]) ";
        //    cmdText += "VALUES (@Channel,@UserId,@UserName,@UserRequest,@BotResponse,@RequestJson,@Created,'',@Created);";

        //    using (SqlCommand cmd = new SqlCommand())
        //    {
        //        cmd.CommandText = cmdText;
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Parameters.Add("@Channel", SqlDbType.VarChar).Value = turnContext.Activity.ChannelId;
        //        cmd.Parameters.Add("@UserId", SqlDbType.VarChar).Value = turnContext.Activity.From.Id;
        //        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = turnContext.Activity.From.Name;
        //        cmd.Parameters.Add("@UserRequest", SqlDbType.NVarChar).Value = turnContext.Activity.Text;
        //        cmd.Parameters.Add("@BotResponse", SqlDbType.NVarChar).Value = answer;
        //        cmd.Parameters.Add("@RequestJson", SqlDbType.NVarChar).Value = json;
        //        cmd.Parameters.Add("@Created", SqlDbType.DateTime).Value = taipeiTime;
        //        using (SqlConnection con = new SqlConnection(_config["SqlConnStr"]))
        //        {
        //            cmd.Connection = con;
        //            con.Open();
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}

        //private void CollectSurveyData(ITurnContext<IMessageActivity> turnContext, string result)
        //{
        //    DateTime timeUtc = DateTime.UtcNow;
        //    TimeZoneInfo taipeiZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
        //    DateTime taipeiTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, taipeiZone);

        //    string cmdText = "WITH temp AS (SELECT MAX(Id)maxid FROM ChatRecords WHERE Channel=@Channel AND UserId=@UserId)";
        //    cmdText += "UPDATE ChatRecords SET SurveyAns=@SurveyAns, Surveyed=@Surveyed ";
        //    cmdText += "FROM ChatRecords INNER JOIN temp ON Id = maxid WHERE SurveyAns='';";

        //    using (SqlCommand cmd = new SqlCommand())
        //    {
        //        cmd.CommandText = cmdText;
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Parameters.Add("@Channel", SqlDbType.VarChar).Value = turnContext.Activity.ChannelId;
        //        cmd.Parameters.Add("@UserId", SqlDbType.VarChar).Value = turnContext.Activity.From.Id;
        //        cmd.Parameters.Add("@SurveyAns", SqlDbType.NVarChar).Value = result;
        //        cmd.Parameters.Add("@Surveyed", SqlDbType.DateTime).Value = taipeiTime;
        //        using (SqlConnection con = new SqlConnection(_config["SqlConnStr"]))
        //        {
        //            cmd.Connection = con;
        //            con.Open();
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}

        //Encoding myEncoding = Encoding.GetEncoding("UTF-8");

        //private void CollectRequestDataGS(ITurnContext<IMessageActivity> turnContext, string answer)
        //{
        //    // Convert UTC Time to Taipei Time
        //    DateTime timeUtc = DateTime.UtcNow;
        //    TimeZoneInfo taipeiZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
        //    DateTime taipeiTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, taipeiZone);

        //    string scriptId = "GoogleScriptId";
        //    string address = $"https://docs.google.com/forms/d/e/{scriptId}/formResponse?";
        //    // DateTime
        //    address += "entry.1=" + HttpUtility.UrlEncode(taipeiTime.ToString("yyyyMMddHHmmss"), myEncoding);
        //    // Source
        //    address += "&entry.2=" + HttpUtility.UrlEncode(turnContext.Activity.ChannelId, myEncoding);
        //    // UserRequest
        //    address += "&entry.3=" + HttpUtility.UrlEncode(turnContext.Activity.Text, myEncoding);
        //    // BotResponse
        //    address += "&entry.4=" + HttpUtility.UrlEncode(answer, myEncoding);
        //    // UserId
        //    address += "&entry.5=" + HttpUtility.UrlEncode(turnContext.Activity.From.Id, myEncoding);
        //    // UserName
        //    address += "&entry.6=" + HttpUtility.UrlEncode(turnContext.Activity.From.Name, myEncoding);
        //    // Json
        //    var json = JsonConvert.SerializeObject(turnContext.Activity);
        //    address += "&entry.7=" + HttpUtility.UrlEncode(json, myEncoding);
        //    address += "&submit=Submit";
        //    SaveData(address);
        //}


        //private string ReadData(string source, string user)
        //{
        //    string sheetId = "GoogleSheetId";
        //    var address = $"https://spreadsheets.google.com/feeds/cells/{sheetId}/1/public/values?alt=json";

        //    //�إ� WebRequest �ë��w�ؼЪ� uri
        //    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(address);
        //    //���w request �ϥΪ� http verb
        //    request.Method = "GET";
        //    request.ContentType = "application/json; charset=utf-8";

        //    var timeString = "";
        //    //�ϥ� GetResponse ��k�N request �e�X
        //    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        //    //�ϥ� GetResponseStream ��k�q server �^�������o��ơAstream �����Q����
        //    using (StreamReader streamreader = new StreamReader(response.GetResponseStream()))
        //    {
        //        timeString = streamreader.ReadToEnd();
        //    }
        //    var list = JObject.Parse(timeString)["feed"]["entry"];
        //    timeString = "";
        //    for (int i = list.Count() - 1; i >= 0; i--)
        //    {
        //        if (list[i]["content"]["$t"].ToString() == source && list[i + 3]["content"]["$t"].ToString() == user)
        //        {
        //            timeString = list[i - 1]["content"]["$t"].ToString();
        //            break;
        //        }
        //    }

        //    return timeString;
        //}

        //private void CollectSurveyDataGS(ITurnContext<IMessageActivity> turnContext, string result)
        //{
        //    string timeString = ReadData(turnContext.Activity.ChannelId, turnContext.Activity.From.Id);

        //    if (timeString.Length > 0)
        //    {
        //        string scriptId = "GoogleScriptId2";
        //        string address = $"https://docs.google.com/forms/d/e/{scriptId}/formResponse?";
        //        // DateTime
        //        address += "entry.1=" + HttpUtility.UrlEncode(timeString, myEncoding);
        //        // Source
        //        address += "&entry.2=" + HttpUtility.UrlEncode(turnContext.Activity.ChannelId, myEncoding);
        //        // UserId
        //        address += "&entry.3=" + HttpUtility.UrlEncode(turnContext.Activity.From.Id, myEncoding);
        //        // Remark
        //        address += "&entry.4=" + HttpUtility.UrlEncode(result, myEncoding);
        //        address += "&submit=Submit";
        //        SaveData(address);
        //    }

        //}

        //// Facebook �D�ʱ���
        //private async void PushFacebookMessage(string messageText)
        //{
        //    Microsoft.Bot.Connector.Authentication.MicrosoftAppCredentials.TrustServiceUrl("https://facebook.botframework.com/");

        //    var botAccount = new ChannelAccount(name: "FacebookBotName", id: "FacebookBotId");
        //    var userAccount = new ChannelAccount(name: "UserName", id: "UserId");
        //    var connector = new ConnectorClient(new Uri("https://facebook.botframework.com/"), _config["MicrosoftAppId"], _config["MicrosoftAppPassword"]);
        //    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);

        //    IMessageActivity message = Activity.CreateMessageActivity();
        //    message.From = botAccount;
        //    message.Recipient = userAccount;
        //    message.Conversation = new ConversationAccount(id: conversationId.Id);
        //    message.Text = messageText;
        //    message.Locale = "en-Us";
        //    await connector.Conversations.SendToConversationAsync((Activity)message);
        //}

    }
}
