<<<<<<< HEAD
# Демографическая база данных Татарстана
На основе пдф файлов предоставленных Росстатом в открытом доступе будет заполнена база данных данными о рождаемости, смертности и миграции\
С сайта bdex.ru будут взяты статистические данные о структуре населения: пол, возраст, образование, занятость и тд
# Начало работы
## Получение данных из истоников
Parcer.cs анализирует данные из пдф файлов и приводит их в краткий и читаемый вид, далее помещает в БД для хранения и дальнейшего использования
# Источники данный
* [Росстат](https://16.rosstat.gov.ru/naselenie) по РТ
* Новостной портал [BDEX](https://bdex.ru/naselenie/respublika-tatarstan/) с данными по насилению Татарстана
* Новостной портал [GOGOV](https://gogov.ru/natural-increase/rt), где присутствуют самые свежие данные родилось/умеро на текущий год
=======
# Демографическая база данных Татарстана
На основе пдф файлов предоставленных Росстатом в открытом доступе будет заполнена база данных данными о рождаемости, смертности и миграции\
С сайта bdex.ru будут взяты статистические данные о структуре населения: пол, возраст, образование, занятость и тд
# Сотовляющие работы
## Получение данных из истоников
Parcer.cs скачивает пдф документ с официального сайта Росстат, анализирует текст и получает из него необходимые строки
## База данных
БД реализована на бесплатйно палтформе supabase с СУБД PostgreSQL
## Backend
В проекте Web реализованно подключение к БД, далее нужно сделать:\ 
* вывод имеющихся данных на сайт
* проверка и обновление данных должно проводиться раз в месяц автоматически 
# Источники данный
* [Росстат](https://16.rosstat.gov.ru/naselenie) по РТ
* Новостной портал [BDEX](https://bdex.ru/naselenie/respublika-tatarstan/) с данными по насилению Татарстана
* Новостной портал [GOGOV](https://gogov.ru/natural-increase/rt), где присутствуют самые свежие данные родилось/умеро на текущий год
>>>>>>> 28523a432f5c8e2a697ebf3215a86d2a366d9f92
