using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace App.Main
{
    public class Handlers
    {
/*        static CancellationTokenSource cts = new CancellationTokenSource();
        static CancellationToken ct;*/
        static HashSet<SalaryObject> data = new HashSet<SalaryObject>();
        static string tmp = "";
        const string usage = "Options:\n" +
                                    "/month                    -   انتخاب ماه\n" +
                                    "/salary                   -   محاسبه حقوق\n" +
                                    "/isInsuredPlus            -   تعیین بیمه تکمیلی\n" +
                                    "/isInsured                -   تعیین بیمه نامین اجتماعی\n" +
                                    "/hours                    -   ساعت کاری\n";
        public static SalaryObject FindOrCreateObj(long chatId)
        {
            SalaryObject obj;
            if ((obj = data.FirstOrDefault(x => x.chatId == chatId)) != null)
                return obj;
            else
            {
                obj = new SalaryObject(chatId);
                data.Add(obj);
                return obj;
            }
        }
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            var action = (message.Text.Split(' ').First()) switch
            {
                "/salary" => CalculateSalary(botClient, message),
                "/salaryHour" => SetSalary(botClient,message),
                "/month" => ChooseMonth(botClient, message),
                "/isInsuredPlus" => ChooseInsuredPlus(botClient, message),
                "/isInsured" => ChooseHoursOfInsured(botClient, message),
                "/hours" => ChooseHours(botClient, message),
                "/start" => Usage(botClient, message),
                _ => Unknown(botClient, message),
            };
            var sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            static async Task<Message> CalculateSalary(ITelegramBotClient botClient, Message message)
            {
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                // Simulate longer running task
                await Task.Delay(500);
                var obj = FindOrCreateObj(message.Chat.Id);
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: obj.ToString());
            }
            static string SetMonth(SalaryObject obj, int month)
            {
                obj.month = month;
                return "با موفقیت اعمال شد.";
            }

            static async Task<Message> ChooseMonth(ITelegramBotClient botClient, Message message)
            {
                /*ct = cts.Token;*/
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                 {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("فروردین","m1"),
                        InlineKeyboardButton.WithCallbackData("اردیبهشت","m2"),
                        InlineKeyboardButton.WithCallbackData("خرداد","m3"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("تیر","m4"),
                        InlineKeyboardButton.WithCallbackData("مرداد","m5"),
                        InlineKeyboardButton.WithCallbackData("شهریور","m6"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("مهر","m7"),
                        InlineKeyboardButton.WithCallbackData("آبان","m8"),
                        InlineKeyboardButton.WithCallbackData("آذر","m9"),
                    },new[]
                    {
                        InlineKeyboardButton.WithCallbackData("دی","m10"),
                        InlineKeyboardButton.WithCallbackData("بهمن","m11"),
                        InlineKeyboardButton.WithCallbackData("اسفند","m12"),
                    },
                });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Choose",
                                                            replyMarkup: inlineKeyboard);
            }
            static async Task<Message> ChooseHours(ITelegramBotClient botClient, Message message)
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                 {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1","1"),
                        InlineKeyboardButton.WithCallbackData("2","2"),
                        InlineKeyboardButton.WithCallbackData("3","3"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("4","4"),
                        InlineKeyboardButton.WithCallbackData("5","5"),
                        InlineKeyboardButton.WithCallbackData("6","6"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("7","7"),
                        InlineKeyboardButton.WithCallbackData("8","8"),
                        InlineKeyboardButton.WithCallbackData("9","9"),
                    },new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Clear","Clear"),
                        InlineKeyboardButton.WithCallbackData("0","0"),
                        InlineKeyboardButton.WithCallbackData("Done","HoursDone"),
                    },
                });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Choose",
                                                            replyMarkup: inlineKeyboard);
            }
            static async Task<Message> SetSalary(ITelegramBotClient botClient, Message message)
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                 {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1","1"),
                        InlineKeyboardButton.WithCallbackData("2","2"),
                        InlineKeyboardButton.WithCallbackData("3","3"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("4","4"),
                        InlineKeyboardButton.WithCallbackData("5","5"),
                        InlineKeyboardButton.WithCallbackData("6","6"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("7","7"),
                        InlineKeyboardButton.WithCallbackData("8","8"),
                        InlineKeyboardButton.WithCallbackData("9","9"),
                    },new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Clear","Clear"),
                        InlineKeyboardButton.WithCallbackData("0","0"),
                        InlineKeyboardButton.WithCallbackData("Done","SetSalary"),
                    },
                });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Choose",
                                                            replyMarkup: inlineKeyboard);
            }
            static async Task<Message> ChooseHoursOfInsured(ITelegramBotClient botClient, Message message)
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                 {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("کامل","full"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1","1"),
                        InlineKeyboardButton.WithCallbackData("2","2"),
                        InlineKeyboardButton.WithCallbackData("3","3"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("4","4"),
                        InlineKeyboardButton.WithCallbackData("5","5"),
                        InlineKeyboardButton.WithCallbackData("6","6"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("7","7"),
                        InlineKeyboardButton.WithCallbackData("8","8"),
                        InlineKeyboardButton.WithCallbackData("9","9"),
                    },new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Clear","Clear"),
                        InlineKeyboardButton.WithCallbackData("0","0"),
                        InlineKeyboardButton.WithCallbackData("Done","InsuredDone"),
                    },
                });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Choose",
                                                            replyMarkup: inlineKeyboard);
            }

            static async Task<Message> Unknown(ITelegramBotClient botClient, Message message)
            {

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "نامعتبر است. برای شروع:\n\n/start",
                                                            replyMarkup: new ReplyKeyboardRemove());
            }



            static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
            {
                FindOrCreateObj(message.Chat.Id);


                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: usage,
                                                            replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private static async Task<Message> ChooseInsuredPlus(ITelegramBotClient botClient, Message message)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("بله","yes"),
                         InlineKeyboardButton.WithCallbackData("خیر","no"),
                    }
                });
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "آیا بیمه تکمیلی هستید؟",
                                                            replyMarkup: inlineKeyboard);
        }

        // Process Inline Keyboard callback data
        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            int number;
            if (callbackQuery.Data == "Clear")
            {
                tmp = "";
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: tmp);

            }
            else if (callbackQuery.Data == "InsuredDone")
            {
                int z = 0;
                foreach (var d in data.Where(w => w.chatId == callbackQuery.From.Id))
                {
                    d.Insured = int.Parse(tmp);
                    z = d.Insured;
                }
                tmp = "";
                await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQuery.Id,
                        text: "اعمال شد.");
                await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "ساعات بیمه تامین اجتماعی رد شده برای شما: " + z + "\n\n\n" + usage);
                await botClient.DeleteMessageAsync(callbackQuery.From.Id, callbackQuery.Message.MessageId);
            }
            else if (callbackQuery.Data == "SetSalary")
            {
                long z = 0;
                foreach (var d in data.Where(w => w.chatId == callbackQuery.From.Id))
                {
                    d.salaryInHour = long.Parse(tmp);
                    z = d.salaryInHour;
                }
                tmp = "";
                await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQuery.Id,
                        text: "اعمال شد.");
                await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "دریافنی شما به ازای هر ساعت: " + z +"تومان "+ "\n\n\n" + usage);
                await botClient.DeleteMessageAsync(callbackQuery.From.Id, callbackQuery.Message.MessageId);
            }
            else if (callbackQuery.Data == "full")
            {

                foreach (var d in data.Where(w => w.chatId == callbackQuery.From.Id))
                {
                    d.Insured = 200;
                }
                tmp = "";
                await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQuery.Id,
                        text: "اعمال شد.");
                await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "ساعات بیمه تامین اجتماعی رد شده برای شما: " + "کامل" + "\n\n\n" + usage);
                await botClient.DeleteMessageAsync(callbackQuery.From.Id, callbackQuery.Message.MessageId);
            }
            else if (callbackQuery.Data == "HoursDone")
            {
                int hours = 0;
                foreach (var d in data.Where(w => w.chatId == callbackQuery.From.Id))
                {
                    d.hours = int.Parse(tmp);
                    hours = d.hours;
                }
                tmp = "";
                await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQuery.Id,
                        text: "اعمال شد.");
                await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "ساعت کاری شما: " + hours + "\n\n\n" + usage);
                await botClient.DeleteMessageAsync(callbackQuery.From.Id, callbackQuery.Message.MessageId);
            }
            else if (int.TryParse(callbackQuery.Data, out number))
            {
                tmp += number;
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: tmp);
            }
            else if (callbackQuery.Data == ".")
            {
                tmp += ".";
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: tmp);
            }
            else if (callbackQuery.Data.StartsWith('m'))
            {
                foreach (var d in data.Where(w => w.chatId == callbackQuery.From.Id))
                {
                    d.month = int.Parse(callbackQuery.Data.Substring(1));
                }
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "اعمال شد.");

                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "اعمال شد.\n\n\n" + usage);
                /*cts.Cancel();*/
                await botClient.DeleteMessageAsync(callbackQuery.From.Id,callbackQuery.Message.MessageId);
                
            }
            else if (callbackQuery.Data == "yes")
            {
                foreach (var d in data.Where(w => w.chatId == callbackQuery.From.Id))
                {
                    d.isInsuredPlus = true;
                }
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "اعمال شد.");
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "اعمال شد.\n\n\n" + usage);
                await botClient.DeleteMessageAsync(callbackQuery.From.Id, callbackQuery.Message.MessageId);
            }
            else if (callbackQuery.Data == "no")
            {
                foreach (var d in data.Where(w => w.chatId == callbackQuery.From.Id))
                {
                    d.isInsuredPlus = false;
                }
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "اعمال شد.");
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "اعمال شد.\n\n\n" + usage);
                await botClient.DeleteMessageAsync(callbackQuery.From.Id, callbackQuery.Message.MessageId);
            }

        }

        private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
        {
            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };

            await botClient.AnswerInlineQueryAsync(
                inlineQueryId: inlineQuery.Id,
                results: results,
                isPersonal: true,
                cacheTime: 0);
        }

        private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
            return Task.CompletedTask;
        }

        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
    }
}
