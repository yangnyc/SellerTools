var webpage = require('webpage');
var url = 'https://www.staplesadvantage.com/SuperCategory?name=Business-Essentials&type=SC&Id=273214';

var page = webpage.create();
page.viewportSize = { width: 2000, height: 2000 };//задаем размер браузера
page.open(url, function (status) {
    if (status === "success") {
        console.log('success');
        console.log(page.content);
        page.render("img.png");
        //выполняем JS и получаем размеры изображения
        var rect = page.evaluate(function () {
            var e = document.querySelector('img');
            if (e != null)
                return e.getBoundingClientRect();//возвращает размер изображения
            return "";
        });
        if (rect !== "") {
            old = page.clipRect;//запоминаем старый
            page.clipRect = rect;//присваиваем новую
            page.render("img.png", { format: 'png', quality: '100' });//сохраняем картинку
            page.clipRect = old;//восстанавливаем старый
        }
        phantom.exit();
    }
    else {
        console.log('not success');
        phantom.exit();
    }
});