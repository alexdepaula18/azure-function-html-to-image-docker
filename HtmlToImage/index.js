// const nodeHtmlToImage = require('node-html-to-image')
const puppeteer = require('puppeteer')
const fs = require('fs');

module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    if (req.body && req.body.htmlBase64) {

        let buff = new Buffer(req.body.htmlBase64, 'base64');
        let text = buff.toString('utf8');

        const browser = await puppeteer.launch({
            headless: true,
            args: ['--no-sandbox']
         });
        const page = await browser.newPage();
        await page.setContent(text);
        await page.screenshot({path: './image.png'});
      
        await browser.close();

        // await nodeHtmlToImage({
        //     output: './image.png',
        //     html: text
        // });

        console.log('The image was created successfully!')

        const image = fs.readFileSync('./image.png');

        context.res = {
            // status: 200, /* Defaults to 200 */
            headers: {
                'Content-Type': "image/png"
            },
            body: new Uint8Array(image)
        };

        context.done();
    }
    else {
        context.res = {
            status: 400,
            body: "Please pass a name on the query string or in the request body"
        };
    }
};