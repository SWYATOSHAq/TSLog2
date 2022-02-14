const createProxyMiddleware = require('http-proxy-middleware');
const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:5217';


// add proxy if you want to pass some api to client app (for example if you add new C# api controller / change version of API / path to api)
const context = [
    "/api/v1/",
];

module.exports = function (app) {
    const appProxy = createProxyMiddleware(context, {
        target: target,
        secure: false
    });

    app.use(appProxy);
};
