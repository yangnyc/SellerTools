import 'react-app-polyfill/ie11';
import 'core-js/features/array/find';
import 'core-js/features/array/includes';
import 'core-js/features/number/is-nan';

var page = require('webpage').create(),
    system = require('system'),
    t, address;

if (system.args.length === 1) {
    console.log('Usage: loadspeed.js <some URL>');
    phantom.exit(1);
} else {
    t = Date.now();
    address = system.args[1];

    page.open(address, function (status) {
        if (status !== 'success') {
            page.injectJs('babel.min.js');
            page.injectJs('node_modules/react-app-polyfill/ie11.js');
            page.injectJs('node_modules/core-js/features/array/find,js');
            page.injectJs('node_modules/core-js/features/array/includes,js');
            page.injectJs('node_modules/core-js/features/number/is-nan.js');

            page.injectJs('polyfill.min.js')
            console.log('FAIL to load the address');
        } else {
            t = Date.now() - t;

            console.log('Page title is ' + page.evaluate(function () {
                return document.body;
            }));
            console.log('Loading time ' + t + ' msec');
        }
        phantom.exit();
    });
}