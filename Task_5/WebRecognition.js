
currentPage = 0
document.getElementById("page").innerText = "Page " + currentPage




function AddImageLeft(name, image) {
    document.getElementById("leftcol").innerHTML += "<li> <div>" + name + "<div/> <img src=\"data:image/jpg;base64," + image + "\" width = 100 height = 100 />"
}

function AddImageRight(name, image) {
    document.getElementById("rightcol").innerHTML += "<li> <div>" + name + "<div/> <img src=\"data:image/jpg;base64," + image + "\" width = 100 height = 100 />"
}

async function GetPage() {

    try {
        var response = await fetch("http://localhost:5000/Recognition/" + currentPage)

        var json = await response.json()

        for (var i = 0; i < parseInt(json.length / 2); i++)
            AddImageLeft("Name: " + json[i].path + " Class: " + json[i].classImage, json[i].imageBase64)

        for (var i = parseInt(json.length / 2); i < json.length; i++)
            AddImageRight("Name: " + json[i].path + " Class: " + json[i].classImage, json[i].imageBase64)

    }
    catch (ex) {
        window.alert(ex)
    }

    if (json.length == 0) {
        ClickPredPage()
    }
        

    document.getElementById("page").innerText = "Page " + currentPage
}

function ClickPredPage() {

    if (currentPage) {
        currentPage--;
        document.getElementById("leftcol").innerHTML = ""
        document.getElementById("rightcol").innerHTML = ""
        GetPage();
    }

}

function ClickNextPage() {
    currentPage++;
    document.getElementById("leftcol").innerHTML = ""
    document.getElementById("rightcol").innerHTML = ""
    GetPage();


}


GetPage()