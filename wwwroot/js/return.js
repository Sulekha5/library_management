$("#checkFine").click(function () {

    var data = {
        studentName: $("#studentName").val(),
        bookName: $("#bookName").val(),
        returnDate: $("#returnDate").val()
    };

    $.ajax({
        url: "/Home/CheckFine",
        type: "POST",
        data: data,
        success: function (res) {
            if (res.message) {
                alert(res.message);
            } else {
                $("#fine").val(res.fine);
            }
        }
    });
});

$("#returnBtn").click(function () {

    var data = {
        studentName: $("#studentName").val(),
        bookName: $("#bookName").val()
    };

    $.ajax({
        url: "/Home/ReturnBook",
        type: "POST",
        data: data,
        success: function (res) {
            if (res.success) {
                alert(res.message);
                location.reload();
            } else {
                alert(res.message);
            }
        }
    });
});