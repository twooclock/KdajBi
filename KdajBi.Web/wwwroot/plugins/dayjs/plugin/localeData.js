!function(n,t){"object"==typeof exports&&"undefined"!=typeof module?module.exports=t():"function"==typeof define&&define.amd?define(t):n.dayjs_plugin_localeData=t()}(this,function(){"use strict";return function(n,t,e){var r=function(n){return n&&(n.indexOf?n:n.s)},o=function(n,t,e,o,u){var a=n.name?n:n.$locale(),i=r(a[t]),s=r(a[e]),f=i||s.map(function(n){return n.substr(0,o)});if(!u)return f;var d=a.weekStart;return f.map(function(n,t){return f[(t+(d||0))%7]})},u=function(){return e.Ls[e.locale()]},a=function(n,t){return n.formats[t]||function(n){return n.replace(/(\[[^\]]+])|(MMMM|MM|DD|dddd)/g,function(n,t,e){return t||e.slice(1)})}(n.formats[t.toUpperCase()])};t.prototype.localeData=function(){return function(){var n=this;return{months:function(t){return t?t.format("MMMM"):o(n,"months")},monthsShort:function(t){return t?t.format("MMM"):o(n,"monthsShort","months",3)},firstDayOfWeek:function(){return n.$locale().weekStart||0},weekdays:function(t){return t?t.format("dddd"):o(n,"weekdays")},weekdaysMin:function(t){return t?t.format("dd"):o(n,"weekdaysMin","weekdays",2)},weekdaysShort:function(t){return t?t.format("ddd"):o(n,"weekdaysShort","weekdays",3)},longDateFormat:function(t){return a(n.$locale(),t)},meridiem:this.$locale().meridiem}}.bind(this)()},e.localeData=function(){var n=u();return{firstDayOfWeek:function(){return n.weekStart||0},weekdays:function(){return e.weekdays()},weekdaysShort:function(){return e.weekdaysShort()},weekdaysMin:function(){return e.weekdaysMin()},months:function(){return e.months()},monthsShort:function(){return e.monthsShort()},longDateFormat:function(t){return a(n,t)},meridiem:n.meridiem}},e.months=function(){return o(u(),"months")},e.monthsShort=function(){return o(u(),"monthsShort","months",3)},e.weekdays=function(n){return o(u(),"weekdays",null,null,n)},e.weekdaysShort=function(n){return o(u(),"weekdaysShort","weekdays",3,n)},e.weekdaysMin=function(n){return o(u(),"weekdaysMin","weekdays",2,n)}}});