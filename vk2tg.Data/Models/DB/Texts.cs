namespace vk2tg.Data.Models.DB
{
    public class Texts
    {
        public const string Help = "Этот бот будет присылать тебе посты из публичных групп VK, на которые ты подпишешься.\n\n" +
                "Для того, чтоб добавить подписку - скопируй имя группы или ссылку на нее из браузера и отправь сюда сообщение в формате:\n" +
                "/subscribe скопированная_тобой_ссылка\n\n" +
                "Для того, чтобы посмотреть список своих подписок используй команду /list\n\n" +
                "Если ты больше не хочешь получать обновления какой-либо группы, напиши:\n" +
                "/unsubscribe имя_группы\n\n" +
                "По всем вопросам обращайся к @immelstorn";

        public const string Welcome = "Добро пожаловать! Для помощи отправь /help";

        public const string AccessDenied =
                "Вероятно, эта группа не публичная или в ней еще нет постов. Извини, этот бот не умеет работать с приватными группами. Если в группе нет постов, подожди пока там появится хотя бы один пост и попробуй еще раз." +
                "\nЕсли считаешь, что произошла ошибка - напиши @immelstorn";

        public const string YouHaveAlreadyStarted = "Ты уже запустил этого бота. Для помощи отправь /help";
        public const string YouAreAlreadySubscribed = "Ты уже подписан на {0}. Либо что-то работает не так. Если считаешь, что произошла ошибка - напиши @immelstorn";
        public const string YouAreSubscribed = "Ты подписался на {0}. Как только появится новый пост, я тебе его пришлю.";
        public const string YouAreNotSubscribed = "Ты не подписан на {0}";
        public const string YouAreUnsubscribed = "Ты отписался от {0}";
        public const string GroupIsNotFound = "Группа {0} не найдена";
        public const string CommandFormatShouldBeNext = "Команда введена неверно. \nФормат команды должен быть таким:\n/subscribe groupname\nили\n/subscribe http://vk.com/groupname";
        public const string YourSubscriptions = "Твои подписки:";
        public const string PleaseUseOnlyOneCommand = "Пожалуйста, используй одну и только одну команду в каждом сообщении. Для помощи отправь /help";
    }
}