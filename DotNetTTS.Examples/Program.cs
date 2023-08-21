using DotNetTTS.Silero;
using DotNetTTS.Vosk;
using Microsoft.Extensions.Logging;

namespace DotNetTTS.Examples
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var logFactory = LoggerFactory.Create(builder =>
                       builder.AddSimpleConsole(options =>
                       {
                           options.IncludeScopes = false;
                           options.SingleLine = true;
                           options.TimestampFormat = "HH:mm:ss ";
                       }).SetMinimumLevel(LogLevel.Information));
            using var speaker1 = new SileroSpeaker("Data\\model.pt", loggerFactory: logFactory, speaker: "eugene", sampleRate: 48000);
            using var speaker2 = new VoskSpeaker("vosk-model-tts-ru-0.1-natasha", logFactory);
            //using var speaker2 = new SileroSpeaker("Data\\model.pt", speaker: "baya", sampleRate: 48000);

            await speaker1.InitializeAsync();
            await speaker2.InitializeAsync();

            //Simple dialog
            await speaker1.SpeakAsync("приветик, как дела?");
            await speaker2.SpeakAsync("привет, хорошо");

            await speaker1.SpeakAsync("что делаешь?");
            await speaker2.SpeakAsync("пью чай");

            //Long dialog
            var dialog = @"
ОНА: Здравствуй!
ОН: Привет!
ОНА: Чего это ты несёшь?
ОН: Несу разные вещи.
ОНА: Несуразные? Почему они несуразные-то?
ОН: Сама ты несуразная, как я погляжу. Разные вещи я несу. Разные! Поняла? Вот, несу мел.
ОНА: Что не сумел?
ОН: Отстань!
ОНА: Да ведь ты говоришь ""не сумел"". Что не сумел-то?
ОН: Мел несу!!! Слушать надо. Несу мел. Мишке. Ему же надо будет.
ОНА: Ну, если ему жена добудет, зачем ты несёшь?
ОН: Жена? Какая жена? Это у Мишки-то жена? Ах ты, шутница! Я сказал: ""Ему же надо будет"". Понадобится, значит.
ОНА: Вон оно что!
ОН: А ещё новость у меня для Мишки приятная: нашлась та марка, которую он так давно искал.
ОНА: Тамарка?
ОН: Ага.
ОНА: И ничего? Симпатичная?
ОН: Красивая. Зелёная такая.
ОНА: Постой, постой... Это что у неё, волосы зелёные что ли?
ОН: У кого волосы?
ОНА: Да у Тамарки!
ОН: У какой Тамарки?
ОНА: Ну, ты же сам сказал: ""Нашлась Тамарка...""
ОН: Та! Марка! Марка, понимаешь, которую Мишка давно ищет. Там арка нарисована!
ОНА: Ага! Всё-таки нарисована Тамарка! Нарисована, да? Так бы и говорил.
ОН: Да отвяжись ты со своей Тамаркой, бестолковая ты голова! Арка там нарисована! Арка! Неужели ты даже этого понять не можешь. Некогда мне!
ОНА: Пока! Смотри, не растеряй свои несуразные вещи.
ОН: Да ну тебя!
ОНА: Да! Стой, стой!
ОН: Ну, что ещё?
ОНА: Привет передавай.
ОН: Кому?
ОНА: Известно кому: Тамарке, Мишке и Мишкиной жене!";

            foreach (var line in dialog.Split('\n').Select(q => q.Trim()))
            {
                Console.WriteLine(line);
                if (line.StartsWith("ОНА:"))
                    await speaker2.SpeakAsync(line.Substring(5));
                else if (line.StartsWith("ОН:"))
                    await speaker1.SpeakAsync(line.Substring(4));
            }
        }
    }
}