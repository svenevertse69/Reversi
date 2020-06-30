const Game = (function(url){

    const privateInit = function (callback){
        var interval = window.setInterval(_getCurrentGameState, 2000);
        callback();
    };

    const _getCurrentGameState = function (){
        Game.Model.getGameState().then(data =>
            $(".grid-container").html(Game.Template.parseTemplate(['bord', "tick"], {bord: data.bord})));

    };

    return {
        init: privateInit,
    };
})("/api/url");
Game.Reversi = (function (){
    let configMap = {
        user: "TEST",
        kleur: "",
        gameId: "",
        kleurAanDeBeurt: 1,
        ready: false,
        stats: [],
    };

    const pas = async function () {

        await Game.Model.getBeurt(configMap.gameId)
            .then(data =>{
                configMap.kleurAanDeBeurt = data;
            });

        if(configMap.kleurAanDeBeurt === configMap.kleur)
        {
            $.ajax({
                url: 'https://localhost:44387/api/Reversi/Pas/' + configMap.gameId,
                type: 'PUT',
                success: async function () {
                    Game.Model.getGameState().then(data =>
                        $(".grid-container").html(Game.Template.parseTemplate(['bord', "bord"], {bord: data.bord})));
                    useFox();
                    await applyStats();
                    Game.Stats.updateC(configMap.stats);
                },
            });
        }
    };

    const useFox = function () {

        var img = document.createElement("img");
        $('#foxImage').empty();

        Game.API.getFoxImage()
            .then(data =>
            {img.src = data.image});
        var src = document.getElementById("foxImage");
        img.width = 100;
        img.height = 100;
        src.appendChild(img);
    };

    const applyStats = async function () {
        Game.Model.getStats(configMap.gameId).then(data => {
            console.log(data);
            configMap.stats = data;
        });
    };


    const checkReady = async function ()
    {
        if(configMap.ready === false){
            var interval = window.setInterval(readyUp, 2000);
        }
    };

    const readyUp = async function () {
         Game.Model.getReady(configMap.gameId)
            .then(data =>{
                configMap.ready = data;
            })
    };

    const showFiche = async function (x, posy, posx){

        await Game.Model.getBeurt(configMap.gameId)
            .then(data =>{
                configMap.kleurAanDeBeurt = data;
            });

        if(configMap.kleurAanDeBeurt === configMap.kleur && configMap.ready === true)
        {

            $.ajax({
                url: 'https://localhost:44387/api/Reversi/Zet/' + Game.Model.configMap.gameId + '/' + posy + '/' + posx,
                type: 'PUT',
                success: async function () {
                    Game.Model.getGameState().then(data =>
                        $(".grid-container").html(Game.Template.parseTemplate(['bord', "bord"], {bord: data.bord})));
                    useFox();
                    await applyStats();
                    Game.Stats.updateC(configMap.stats);
                },
            });
        }
        else{
            console.log("NIET AAN DE BEURT!");
        }
    };

    const privateInit = async function (){

        Game.Model.getGameState().then(data =>
            $(".grid-container").html(Game.Template.parseTemplate(['bord', "bord"], {bord: data.bord})));

        await Game.Model.getUser().then(data => {
            configMap.user = data.id;
        });

        await checkReady();

        await  Game.Model.getGameState().then(data => {
            configMap.gameId = data.spelID;
        });

        await Game.Model.getKleur(configMap.user, configMap.gameId).then(data => {
            configMap.kleur = data;
        });

        await useFox();

        await applyStats();
        Game.Stats.drawChart();
        Game.Stats.updateC(configMap.stats);

    };
    return {
        init: privateInit,
        showFiche: showFiche,
        pas: pas,
        useFox: useFox,
    };
})();

Game.Data = (function ($){
    console.log("hallo, vanuit module Data");

    const configMap = {
        apiKey: "1287dd0fbc39851a31b3d66be3df19a0",
        mock: [
            {
                url: "https://localhost:44308/api/Reversi/Beurt/567",
                data: 2
            }
        ]
    };

    let stateMap = {
        environment: "production"
    };

    const getMockData = function(url){

        const mockData = configMap.mock.find(item => item.url === url).data;

        return new Promise((resolve, reject) =>
        {
            resolve(mockData);
        });

    };

    const get = function(url){

        if(checkEnvironment){
            if(stateMap.environment === "development"){
                return getMockData(url);
            }
            else if(stateMap.environment === "production"){
                return $.get(url)
                    .then(r => {
                        return r;
                    })
                    .catch(e =>{
                        console.log(e.message);
                    })
            }
        }
    };

    let checkEnvironment = new Promise(function(resolve, reject){
        if(stateMap.environment === "development" ){
            return resolve(stateMap.environment);
        }
        else if(stateMap.environment === "production"){
            return resolve(stateMap.environment);
        }
        else{
            return reject("Wrong environment");
        }
    })
        .catch(function () {
            return new Error("Wrong environment");
        });

    const privateInit = function (environment) {
        console.log("private data");
        stateMap.environment = environment;

    };
    return{
        init: privateInit,
        getMockData,
        get,
        checkEnvironment: checkEnvironment,
        configMap
    };
})(jQuery);
Game.Model = (function (){
    const privateInit = function () {
        console.log("private Model");
    };

    let configMap = {
        gameId: "",
    };

    const getWeather = function () {
        Game.Data.get("http://api.openweathermap.org/data/2.5/weather?q=zwolle&apikey=" + Game.Data.configMap.apiKey)
            .then(data => {
                console.log(data.main.temp);
            })
            .catch(e =>
            {
                console.log(e.message);
            });
    };

    const _getGameState = function (){

        var full_url = document.URL;
        var url_arr = full_url.split('=');
        var last_segment = url_arr[url_arr.length-1];
        configMap.gameId = last_segment;

        return Game.Data.get("https://localhost:44387/api/Reversi/" + last_segment)
            .then(data => {
                return data;
            });
    };

    const _getUser = function () {
        return Game.Data.get('https://localhost:44387/api/Reversi/GetUser')
            .then(data => {
                return data;
            });
    };

    const _getKleur = function (user, gameId) {
        return Game.Data.get('https://localhost:44387/api/Reversi/GetKleur/' + user + '/' + gameId)
            .then(data => {
                return data;
            });
    };

    const _getBeurt = function (gameId) {
        return Game.Data.get('https://localhost:44387/api/Reversi/Beurt/' + gameId)
            .then(data =>{
                return data;
            })
    };

    const _getReady = function (gameId) {
        return Game.Data.get('https://localhost:44387/api/Reversi/Ready/' + gameId)
            .then(data =>{
                return data;
            })
    };

    const _getStats = function (gameId) {
        return Game.Data.get('https://localhost:44387/api/Reversi/Stats/' + gameId)
            .then(data =>{
                return data;
            })
    };


    return{
        init: privateInit,
        getGameState : _getGameState,
        getUser: _getUser,
        getKleur: _getKleur,
        getBeurt: _getBeurt,
        getWeather,
        configMap: configMap,
        getReady: _getReady,
        getStats: _getStats,
    }
})();
Game.Template = (function (){

    const _getTemplate = function (templateName){
        let template = spa_templates.templates;
        templateName.forEach(path => template = template[path]);
        return template;
    };

    const _parseTemplate = function (templateName, data){
        return _getTemplate(templateName)(data);
    };

    return {
        getTemplate: _getTemplate,
        parseTemplate: _parseTemplate
    };
})();

class FeedbackWidget{
    constructor(elementId) {
        this._elementId = elementId;
    }

    get elementId() {
        return this._elementId;
    }

    show(message, type){
        var x = document.getElementById(this.elementId);
        x.style.display = "block";
        $(x).html(message);
        var object = {
            message: message,
            type: type,
        };
        if(type != null) {
            this.log(object);
        }
        if(type === "success"){
            $(x).addClass("alert alert-success");
        }
        else{
            $(x).addClass("alert alert-danger");
        }
    }

    hide(){
        var x = document.getElementById(this.elementId);
        $(x).addClass("success--fadeOut");
        x.addEventListener("animationend", function() { x.style.display = "none" }, false);
        console.log("hidden");
    }

    log(message){
        this.message = message;
        var existing = localStorage.getItem("feedback-widget");
        if(existing != null){
            var data = JSON.parse(existing);
            if(data.length > 9){
                data.shift();
            }
            data.push(message);
            existing = localStorage.setItem("feedback-widget", JSON.stringify(data));
        }
        else{
            localStorage.setItem("feedback-widget", JSON.stringify([message]));
        }
    }

    removeLog(){
        localStorage.removeItem("feedback-widget");
    }

    history () {
        var existing = localStorage.getItem("feedback-widget");
        var data = JSON.parse(existing);
        var dataString = "";

        for(let i = 0; i < data.length; i ++) {
            dataString += data[i].type + " - " + data[i].message + "<br>";
        }
        this.show(dataString);
    }
}
Game.API = (function(){
    const privateInit = function () {
        console.log("private Model");
    };

    const _getFoxImage = function () {
        return Game.Data.get("https://randomfox.ca/floof/")
            .then(data => {
                return data;
            })
    };


    return {
        init: privateInit,
        getFoxImage: _getFoxImage,
    }
})();
Game.Stats = (function () {

    let configMap = {
        chart: "",
    };

    const privateInit = function (stats) {
        drawChart(stats);
    };

    const drawChart = function () {
        //$('#myChart').empty();

        var ctx = document.getElementById('myChart').getContext('2d');
        configMap.chart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: ['Black', 'White'],
                datasets: [{
                    label: 'Aantal fiches op het spelbord',
                    data: [],
                    backgroundColor: [
                        'rgba(0, 0, 0, 1)',
                        'rgba(255, 250, 250, 1)',
                    ],
                    borderColor: [
                        'rgba(0, 0, 0, 1)',
                        'rgba(255, 10, 10, 1)',
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: false,
                animation: {
                  duration: 0,
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: false,
                        }
                    }]
                }
            }
        });
    };

    const updateC = function (data) {
        console.log(data);

        let newSet = {
            label: 'Aantal fiches op het spelbord',
            data: [data[1], data[0]],
            backgroundColor: [
                'rgba(0, 0, 0, 1)',
                'rgba(255, 250, 250, 1)',
            ],
            borderColor: [
                'rgba(0, 0, 0, 1)',
                'rgba(255, 10, 10, 1)',
            ],
        };

        configMap.chart.data.datasets.push(newSet);
        configMap.chart.update();
    };

    return {
        init: privateInit,
        drawChart: drawChart,
        updateC: updateC,
    }

})();