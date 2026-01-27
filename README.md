# SolyankaGuide
Гид по «Народной Солянке» OGSR 2016 (2026)

<img width="640" height="360" alt="image" src="https://github.com/user-attachments/assets/3fa27443-e4b0-4be1-86bc-50a60f83a00f" />

Системные требования:
- Windows 7+ / Linux с Wine 11.0
- .NET Destkop Runtime 6.0.3.6

Тестировалось на Windows 10/11 и Ubuntu 24.04

Инструкция по сборке:
- Клонировать репозиторий
- Внести изменения в файлы в каталоге /Assets
- Настроить .json файлы с текстом (faq.json, mech.json и т.д.)
- Установить все .json файлы и картинки КРОМЕ hashes.json как "Содержимое" с копированием в выходной каталог
- Запустить HashBuilder.exe для обновления файла hashes.json
- Загрузить коммит на гитхаб
- Собрать билд (желательно под .NET 6.0)
- Запаковать билд в zip и выложить релиз
