// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

//將專案名稱替代成自己的專案名稱，類似 namespace EchoBot1.Bots
namespace 專案名稱.Bots
{
    public class EchoBot : ActivityHandler
    {
        // 載入設定檔
        private readonly IConfiguration _config;
        public EchoBot(IConfiguration config)
        {
            _config = config;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // 建立要傳回訊息到用戶端的連接器 (ConnectorClient)
            var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), _config["MicrosoftAppId"], _config["MicrosoftAppPassword"]);
            // 建立用來回覆訊息的活動物件 (Activity)
            var reply = (turnContext.Activity as Activity).CreateReply();
            // 取得從用戶端傳來的文字/問題 userWords
            string userWords = turnContext.Activity.Text;
            // predictionResult 用來紀錄 QnA 或 Luis 的回答
            string predictionResult = "";
            // Part06 部分：是否建立問卷按鈕 (<<很有用>>等)
            bool createButtons = false;
            // Part06 部分：是否是用戶端回答問卷
            bool answerSurvey = false;

            // 先判斷用戶端傳來的不是空字串或空白
            if (!string.IsNullOrWhiteSpace(userWords))
            {
                if (userWords.StartsWith("<<") && userWords.EndsWith(">>"))
                {
                    // 如果是回答問卷，則謝謝用戶
                    reply.Text = "謝謝您！歡迎繼續發問喔！";
                    answerSurvey = true;
                }
                else
                {
                    // 將用戶端文字丟入 GetLUISPrediction
                    // 如果不是 None，就依照 Luis 分析結果產生固定的回應方式 (OK，你想要...)
                    var luisPredction = await GetLUISPrediction(userWords);
                    if (luisPredction["Intent"] != "None")
                    {
                        predictionResult = "OK，你想要" + luisPredction["Intent"] + "，" + luisPredction["Entity"];
                        reply.Text = predictionResult;
                    }
                    else
                    {
                        // 如果 Luis 取得的 intent 意向為 None，接著丟到問答知識庫繼續分析
                        // 從 QnA maker 取得答案: 將用戶端文字 userWords 丟進剛才建立的 GetQnAResponse 方法
                        predictionResult = await GetQnAResponse(userWords);
                        // 如果答案不是「No good match found in KB.」，表示問答知識庫裡有找到適合的配對，就設定回覆的文字為 QnA 拿到的答案
                        if (predictionResult != "No good match found in KB.")
                        {
                            reply = (turnContext.Activity as Activity).CreateReply(predictionResult);
                        }
                    }
                }

                // 如果先前的流程中沒有找到適合的答案配對，就回答下面的文字
                if (reply.Text.Length == 0)
                {
                    reply.Text = "不好意思，機器人客服無法判斷您的意思，請重新說明您的問題";
                }

                // 如果頻道是 line，就用 LineBotSDK 的 reply 方式 (ReplyMessage) 回應，其他頻道不變
                if (turnContext.Activity.ChannelId.ToLower() == "line")
                {
                    // LINE Templates 手機上無法顯示
                    // LINE ButtonsTemplate 有字數限制 (約50字) ，所以先將回應格式化成純文字(去除html標籤等)，只保留主要的回應內容
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
                    // 透過連接器回傳訊息給用戶
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }

            if (answerSurvey)
            {
                // 將用戶回饋/問卷結果儲存到資料表
                CollectSurveyData(turnContext, userWords);
            }
            else
            {
                // 將對話存到 SQL 資料庫
                CollectRequestData(turnContext, predictionResult);
            }

            // 註解預設的 Echo: 回應方式
            //await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
        }

        // 處理有用戶新加入對話的歡迎訊息，不需要這段，可先註解
        //protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        //{
        //    foreach (var member in membersAdded)
        //    {
        //        if (member.Id != turnContext.Activity.Recipient.Id)
        //        {
        //            await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
        //        }
        //    }
        //}


        // 建立將「question」文字傳入後，會得到 QnA 答案的方法 (透過 WebAPI 和問答知識庫溝通)
        private async Task<string> GetQnAResponse(string question)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(_config["QnAUri"]);
                request.Content = new StringContent("{question:'" + question + "'}", Encoding.UTF8, "application/json");

                // 注意 "EndpointKey " 字尾有個空白在
                request.Headers.Add("Authorization", "EndpointKey " + _config["QnAEndpointKey"]);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                // 解析 QnA maker 傳回來的 json 資料
                return JObject.Parse(responseBody)["answers"][0]["answer"].ToString();
            }
        }

        // Luis只負責解析文字，回應方式須再依解析結果另行設計
        // 此範例 GetLUISPrediction 中將文字 text 丟入此方法中，最後產生一個字典來存放 Luis 丟回的 Intents 和 Entities
        private async Task<Dictionary<string, string>> GetLUISPrediction(string text)
        {
            using (var client = new HttpClient())
            {
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config["LuisKey"]);
                // Request parameters
                queryString["verbose"] = "true";
                queryString["staging"] = "true";
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

        // 將對話存到 SQL 資料庫
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

        // 將調查結果和回覆時間更新到 SQL 資料表
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
                cmd.Parameters.Add("@SurveyAns", SqlDbType.NVarChar).Value = result;
                cmd.Parameters.Add("@Surveyed", SqlDbType.DateTime).Value = taipeiTime;
                using (SqlConnection con = new SqlConnection(_config["SqlConnStr"]))
                {
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Facebook 主動推播：給訊息文字messageText後，依照此連接器的設定傳給特定的用戶
        private async void PushFacebookMessage(string messageText)
        {
            // 如果是 line，將後面括號內的 TrustServiceUrl 改為 "https://line.botframework.com/"
            Microsoft.Bot.Connector.Authentication.MicrosoftAppCredentials.TrustServiceUrl("https://facebook.botframework.com/");

            // 在 SQL 資料庫的 RequestJson 欄位可找到所需參數
            // 機器人資訊
            var botAccount = new ChannelAccount(name: "FacebookBotName", id: "FacebookBotId");
            // 用戶端資訊
            var userAccount = new ChannelAccount(name: "UserName", id: "UserId");
            // 如果是 line，將括號內的 Uri 改為 "https://line.botframework.com/"
            var connector = new ConnectorClient(new Uri("https://facebook.botframework.com/"), _config["MicrosoftAppId"], _config["MicrosoftAppPassword"]);
            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);

            IMessageActivity message = Activity.CreateMessageActivity();
            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(id: conversationId.Id);
            message.Text = messageText;
            message.Locale = "en-Us";
            await connector.Conversations.SendToConversationAsync((Activity)message);
        }

    }
}
