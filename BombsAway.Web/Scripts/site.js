$(document).ready(function () {
    $('select.hex').css('background-color', $('div.data-hex[data-id="' + $('select.hex').val() + '"]').data('color'));

    $('select.hex option').each(function () {
        var id = $(this).val();

        $(this).css('background-color', $('div.data-hex[data-id="' + id + '"]').data('color'));
    });

    $('select.hex').change(function (ev) {
        var id = $(this).val();

        $(this).css('background-color', $('div.data-hex[data-id="' + id + '"]').data('color'));
    });
});