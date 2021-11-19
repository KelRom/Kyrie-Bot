using Discord;
using Discord.Commands;
using System.IO;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kryie_Bot
{



    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        string path = Directory.GetCurrentDirectory() + "\\suggestions.txt";
        //Discord name for emoji and unicode from emoji website
        string imgUrl = "https://cdn.discordapp.com/icons/852682277464178738/3c4e30d626fecfc5fe996d04b000e7d0.png?size=96";

        static string[,] reaction = new string[,] {
            {":one:","\u0031\uFE0F\u20E3"},
            {":two:","\u0032\uFE0F\u20E3" },
            {":three:","\u0033\uFE0F\u20E3" },
            {":four:","\u0034\uFE0F\u20E3" },
            {":five:","\u0035\uFE0F\u20E3" },
            {":six:","\u0036\uFE0F\u20E3" },
            {":seven:","\u0037\uFE0F\u20E3" },
            {":eight:","\u0038\uFE0F\u20E3" },
            {":nine:","\u0039\uFE0F\u20E3" },
            {":keycap_ten:","🔟"}};

        static Emoji one = new Emoji(reaction[0, 1]);
        static Emoji two = new Emoji(reaction[1, 1]);
        static Emoji three = new Emoji(reaction[2, 1]);
        static Emoji four = new Emoji(reaction[3, 1]);
        static Emoji five = new Emoji(reaction[4, 1]);
        static Emoji six = new Emoji(reaction[5, 1]);
        static Emoji seven = new Emoji(reaction[6, 1]);
        static Emoji eight = new Emoji(reaction[7, 1]);
        static Emoji nine = new Emoji(reaction[8, 1]);
        static Emoji ten = new Emoji(reaction[9, 1]);
        Emoji[] eArray = new Emoji[10]
            {one,
            two,
            three,
            four,
            five,
            six,
            seven,
            eight,
            nine,
            ten
            };


        public bool requiredRole(SocketUser user)
        {
            bool isStaff = false;
            var membersWithStaffRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == "staff").Members;
            foreach (var member in membersWithStaffRole)
            {
                if (member.Username == user.Username) isStaff = true;

            }
            return isStaff;
        }


        static Embed make_embed(string titlePollNum, string descriptionQuestion, string[] fieldValues, string imageUrl = null, string author = null)
        {
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = titlePollNum,
                Description = descriptionQuestion,
                ThumbnailUrl = imageUrl,
                Color = Color.Gold

            };
            if (author != null)
            {
                embed.WithAuthor($" Was created by: {author} ");
                embed.WithCurrentTimestamp();
            }
            if (fieldValues != null)
            {
                for (int i = 0; i < fieldValues.Length; i++)
                {
                    embed.AddField(reaction[i, 0], fieldValues[i]);
                }
            }
            return embed.Build();
        }


        [Command("help")]
        [Summary("Display all the commands that can be used")]
        async Task HelpCommand()
        {
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = "Commands",
                ThumbnailUrl = imgUrl,
                Color = Color.DarkRed
            };

            foreach (var cmd in EventCommandHandler.commands.Commands)
            {
                string alliases = null;
                foreach (var allias in cmd.Aliases)
                {
                    alliases += allias + "\n";
                }
                embed.AddField($"{cmd.Name} \nAlliases-{alliases}", $"\n{cmd.Summary}");
            }
            await ReplyAsync(embed: embed.Build());
        }


        [Command("clear")]
        [Summary("Will clear messages up to 14 days, Default will delete 100 message if there is less than that nothing will get deleted. You can put how many messages you would like to get deleted.")]
        async Task ClearMessages(int num = 100)
        {
            var items = await Context.Channel.GetMessagesAsync(num).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(items);
        }


        [Command("report_bug")]
        [Alias("bug", "create_trouble_ticket", "create_ticket", "report_issue")]
        [Summary("You will be able to enter in an issue found with the game. A DM will be created with the Founder or one of the staff member to further request information on where it was found.")]
        async Task TicketSystem([Remainder] string report = null)
        {
            //Get the input of the user
            if (report == null)
            {
                await ReplyAsync("Please provide a brief description of what the issue is. Preferably 15 words or less. ");
            }
            else
            {

                //Send the user a message saying we will assist them shortly
                await Context.Client.GetUser(Context.User.Id).SendMessageAsync("Thank you for submitting a ticket it has been added to the ticket channel and a message has been sent to our staff members. One will be with you shortly to get more information on the issue");

                //get the user/person who made the ticket
                string userName = Context.User.Username;

                // Add the ticket to text channel that has all the tickets as an embed
                // make an embed with title being the ticket number, Author being the person who sent the request, Description will be what the user sent, add timestamp to show when the request was made 
                Random rand = new Random();
                int num = rand.Next(1000, 5000);
                ulong ticketChannelID = (ulong)(Context.Guild.Channels.FirstOrDefault(chan => chan.Name == "tickets")?.Id);
                await Context.Guild.GetTextChannel(ticketChannelID).SendMessageAsync(embed: make_embed($"Ticket#{num}", report, null, null, userName));

                //Notify the owner or a member that has the role staff
                var membersWithStaffRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == "staff").Members;
                foreach (SocketGuildUser member in membersWithStaffRole)
                {
                    await Context.Client.GetUser(member.Id).SendMessageAsync($"{userName} has submitted a trouble ticket. Please find them in the server and DM them to get more information. All staff members have been notified");
                }
            }
        }


        [Command("close_ticket")]
        [Alias("close_trouble_ticket", "close_bug_report", "close_issue", "close_report")]
        [Summary("Allowed required memmbers, staff, to be able to close a trouble ticket once it has been fixed or attended to.")]
        async Task CloseTicket(ulong messageID = 0)
        {
            if (!requiredRole(Context.User))
            {
                await ReplyAsync("In order to use this command you must have the role of staff.");
            }
            else if (messageID == 0) await ReplyAsync("Please provide a valid message ID in order to close the proper ticket. ");
            else
            {
                ulong channel = Context.Guild.Channels.FirstOrDefault(chan => chan.Name == "tickets").Id;
                await Context.Guild.GetTextChannel(channel).DeleteMessageAsync(messageID);
                var msg = await ReplyAsync("The ticket has been closed an erased from the tickets channel");
                await Task.Delay(2500);
                await Context.Channel.DeleteMessageAsync(msg.Id);
            }
        }


        [Command("add_category")]
        [Summary("Allows required members, staff, to be able to categories within the server. Must add the name you would like if not a message will prompt you to enter the name.")]
        async Task AddCategory([Remainder] string name = null)
        {
            if (!requiredRole(Context.User)) await ReplyAsync("You must be a staff member in order to use this command.");
            else if (name == null)
            {
                await ReplyAsync("Please enter in a name for the channel you would like.");
            }
            else await Context.Guild.CreateCategoryChannelAsync(name);
        }


        [Command("add_text_channel_by_cat_name")]
        [Alias("ATCBCN")]
        [Summary("Required members, staff, will be able to add a text channel(s) within a category. Call the commmand put in the category then a name(s) of channel. Separate by comma, use only if name of category is one word, otherwise you other command ATCBCID. ")]
        async Task AddTextChannel(string category = null, [Remainder] string textChannels = null)
        {
            if (!requiredRole(Context.User)) await ReplyAsync("You must be a staff member in order to use this command.");
            else if (category == null || textChannels == null)
            {
                await ReplyAsync("Please enter the category channel name, to add the text channels into, and the text channels you would like to add. EX. -atcbcn wonderful textchannel1, textchanne2, textchannel3");
            }
            else
            {
                string[] textChannel = textChannels.Substring(0).TrimStart().Split(',');
                ulong catID = (ulong)(Context.Guild.CategoryChannels.FirstOrDefault(cat => cat.Name.Equals(category))?.Id);
                foreach (string channel in textChannel)
                {
                    await Context.Guild.CreateTextChannelAsync(channel, prop => prop.CategoryId = catID);
                }
            }
        }


        [Command("add_text_channel_by_cat_ID")]
        [Alias("ATCBCID")]
        [Summary("Required members, staff, will be able to add a text channel(s) within a category. Call the commmand put in the category then a name(s) of channel. This will need the category id.")]
        async Task AddTextChannel(ulong id = 0, [Remainder] string textChannels = null)
        {
            if (!requiredRole(Context.User)) await ReplyAsync("You must be a staff member in order to use this command.");
            else if (id == 0 || textChannels == null)
            {
                await ReplyAsync("Please enter the category ID to add the text channels into, and the text channels you would like to add. -atcbcid 1234567890 textchannel1, textchanne2, textchannel3 ");
            }
            else
            {
                string[] textChannel = textChannels.Substring(0).TrimStart().Split(',');
                foreach (string channel in textChannel)
                {
                    await Context.Guild.CreateTextChannelAsync(channel, prop => prop.CategoryId = id);
                }
            }
        }


        [Command("make_suggestions")]
        [Alias("create_suggestion", "add_suggestion", "suggest", "make_suggestion", "suggestion", "add_suggest")]
        [Summary("Call the command and add what you would like to suggest to the developers. It will be added to the suggestion channel. If you made a mistake you can use edit_suggestion")]
        async Task makeSuggestions([Remainder] string message = null)

        {

            if (message == null)
            {
                await ReplyAsync("Please enter the suggestion you would like to make. ");
                return;
            }


            File.AppendAllText(path, $"{Context.User}, {message}\n");
            string[] currentSuggestions = File.ReadAllLines(path);
            List<string> usersNames = new List<string>();
            foreach(string suggestion in currentSuggestions)
            {
                usersNames.Add(suggestion.Substring(0, suggestion.IndexOf(',')));
            }

            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = "Suggestions",
                ThumbnailUrl = imgUrl,
                Description = "What the community would like to see added or improved on.",
                Color = Color.Blue
            };

            for (int i = 0; i < currentSuggestions.Length; i++)
            {
                embed.AddField(new EmbedFieldBuilder().WithName($"{i + 1}.  {usersNames[i]} suggested:").WithValue(currentSuggestions[i].Substring(currentSuggestions[i].IndexOf(',') + 1)));
            }
            ulong suggestionChannelID = (ulong)Context.Guild.TextChannels.FirstOrDefault(channel => channel.Name.Equals("suggestions"))?.Id;
            await Context.Guild.GetTextChannel(suggestionChannelID).SendMessageAsync(embed: embed.Build());

        }


        [Command("edit_suggestion")]
        [Alias("edit_suggest", "change_suggest", "change_suggestion")]
        [Summary("You will be able to edit a suggestion but entering the number you would like to edit and providing the new suggestion.")]
        async Task editSuggestion(int suggestionNumber = 0, [Remainder] string editedSuggestion = null)
        {
            
            if (suggestionNumber < 0 || editedSuggestion == null)
            {
                await ReplyAsync("Please provide a valid number and follow that with the edit you would like to be made. Numbers are shown next to each suggestion. ");
                return;
            }
            string[] currentSuggestions = File.ReadAllLines(path);
            List<string> userNames = new List<string>();
            
            foreach(string suggestion in currentSuggestions)
            {
                userNames.Add(suggestion.Substring(0, suggestion.IndexOf(',')));
            }

            if (Context.User.ToString() == userNames[suggestionNumber -1])
            {
                currentSuggestions[suggestionNumber-1] = $"{Context.User}, {editedSuggestion}";
                File.WriteAllLines(path, currentSuggestions);

                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = "Suggestions",
                    ThumbnailUrl = imgUrl,
                    Description = "What the community would like to see added or improved on.",
                    Color = Color.Blue
                };

                for (int i = 0; i < currentSuggestions.Length; i++)
                {
                    embed.AddField(new EmbedFieldBuilder().WithName($"{i + 1}.  {userNames[i]} suggested:").WithValue(currentSuggestions[i].Substring(currentSuggestions[i].IndexOf(',') + 1).TrimStart()));
                }
                ulong suggestionChannelID = (ulong)Context.Guild.TextChannels.FirstOrDefault(channel => channel.Name.Equals("suggestions"))?.Id;
                await Context.Guild.GetTextChannel(suggestionChannelID).SendMessageAsync(embed: embed.Build());

            }
            else 
            {
                await ReplyAsync("You were not the user that made that suggestion. Sorry look for your suggestion. Thank You!!!");
            }
            

        }


        [Command("make_poll")]
        [Alias("create_poll", "poll")]
        [Summary("Required member, staff, will be able to make poll. Type if the question of the poll followed but a question mark, after question mark put in fields you would like, separate options with commas to note end of option. Will make an embed and add reactions for users to react to. ")]
        async Task MakePoll([Remainder] string message = null)
        {
            if (!requiredRole(Context.User)) await ReplyAsync("You must be a staff member in order to use this command.");
            else if (message == null) await ReplyAsync("Please enter your poll question followed by a question mark, then add your options after the question mark. Separate everything by commas, Ex. What would you like to eat today? Pizza, Eggs, Waffles");
            else if (!message.Contains("?")) await ReplyAsync("There was no question mark located within your message. Please add a question mark in order to split you question from your options. Thank you!!");
            else
            {
                Random ran = new Random();
                int num = ran.Next(1000, 2000);
                int questionMarkLocation = message.IndexOf("?") + 1;
                string question = message.Substring(0, questionMarkLocation);
                string[] options = message.Substring(questionMarkLocation).TrimStart().Split(',');
                for (int i = 0; i < options.Length; i++)
                {
                    options[i] = options[i];
                }
                var msg = await Context.Channel.SendMessageAsync(embed: make_embed($"Poll#{num} ", question, options, imgUrl));
                for (int i = 0; i < options.Length; i++)
                {
                    await msg.AddReactionAsync(eArray[i]);
                }
            }
        }


        [Command("close_poll")]
        [Summary("Required members, staff, will be able to close the poll and the results will be displayed.")]
        async Task ClosePoll(string id = null)
        {
            if (!requiredRole(Context.User)) await ReplyAsync("You must be a staff member in order to use this command.");
            else if (id == null) await Context.Channel.SendMessageAsync("Please provide a message id. Right click on the poll and click copy id.");
            else
            {
                ulong idNum = ulong.Parse(id);
                IMessage msgToClose = await Context.Channel.GetMessageAsync(idNum);
                var reaction = msgToClose.Reactions;
                var title = msgToClose.Embeds.FirstOrDefault(emd => emd.Title != null).Title;
                var fields = msgToClose.Embeds.FirstOrDefault(emb => emb.Fields != null).Fields;

                EmbedBuilder closePollEmbed = new EmbedBuilder()
                {
                    Title = $"Results of {title}",
                    Description = "Results are in!! drum roll please......",
                    ThumbnailUrl = imgUrl
                };

                string[] fieldArray = new string[reaction.Count];
                for (int i = 0; i < fieldArray.Length; i++)
                {
                    fieldArray[i] = fields[i].Value;
                }

                int iterator = 0;
                foreach (KeyValuePair<IEmote, ReactionMetadata> kvp in reaction)
                {
                    closePollEmbed.AddField($"{fieldArray[iterator]} had: ", $"{kvp.Value.ReactionCount - 1} votes ");
                    iterator++;
                }
                await msgToClose.RemoveAllReactionsAsync();
                await ReplyAsync(embed: closePollEmbed.Build());
            }
        }
    }
}
