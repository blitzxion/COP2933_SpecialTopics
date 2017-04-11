if (!window.jQuery) {
	throw "jQuery is required for this script file."
}

function LoadMetrics(url, onSuccess) {
	$.ajax({
		url: url,
		dataType: 'json',
		success: function (data) {
			var jData = JSON.parse(data);

			if (onSuccess)
				onSuccess(jData);
		}
	});
}

$.fn.hasData = function (key) {
	return (typeof $(this).data(key) != 'undefined');
}