
var JabbREto = (function(){

var pub = {
	messages: [],
	addMessage: function(msg) {
		var msgContent = $( "#template-" + msg.Type ).render(msg);

		var m = $('#messages');
		if (msg.IsHistory) {
			this.messages.splice(0, msg);
			m.prepend(msgContent);
		}
		else {
			this.messages.splice(this.messages.length, msg);
			m.append(msgContent);
		}
		m.prop({ scrollTop: m.prop('scrollHeight') });
	}
};

return pub;

}());
