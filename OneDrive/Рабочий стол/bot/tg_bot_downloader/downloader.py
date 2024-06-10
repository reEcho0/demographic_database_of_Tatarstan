import telebot
from telebot import types
import logging
token = "6662883485:AAGLEG4QTov-88eldGG_IfnfNcBUpWdRt3o"
bot = telebot.TeleBot(token)
logging.basicConfig(level=logging.INFO, filename="py_log.log", filemode="w",
                    format="%(asctime)s %(levelname)s %(message)s")

@bot.message_handler(commands=['start'])
def bot_start(message):
    bot.send_message(message.from_user.id,
                     "Привет!")
    bot_button(message)


@bot.message_handler(content_types=['text'])
def bot_button(message):
    logging.info('bot_button')
    markup = types.InlineKeyboardMarkup()
    b_down_video = types.InlineKeyboardButton('Скачать видео',callback_data='download_video')
    markup.add(b_down_video)
    bot.send_message(message.from_user.id,'Что будем делать?',reply_markup=markup)
    if message.text == 'Скачать видео':
        logging.info('download_video')
        download_video()


@bot.callback_query_handler(func=lambda call: True)
def download_video(call):
    logging.info('def down_vid')
    if call.data == 'download_video':
        logging.info(call.from_user.id)
        with open('video.mp4', 'rb') as vf:
            bot.send_video(call.from_user.id, vf)
        logging.info('send video')
    return 0


bot.polling(none_stop=True, interval=0)
