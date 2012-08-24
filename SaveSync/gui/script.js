function uploadProgress(progress) {
    var progressBar_root = $("#progress_bar");
    $(".ui-progress", progressBar_root).animateProgress(progress, null);
}

function gameProgress(progress, total, gamename) {
    $("#gamename").text(gamename);
    $("#btnBackup").text("Backing up games: " + progress + " of " + total);
}

function disableButton() {
    $(".a_demo_five").addClass('disabled');
}

$(document).ready(function () {
    $('#fancyClock').tzineClock();
});