var fs = require('fs');
var esprima = require('esprima');
const express = require('express')
var bodyParser = require('body-parser');
const app = express()
app.use(bodyParser.urlencoded({ extended: false }))
app.use(bodyParser.json())

function readFile(obj){
    return new Promise((resolve, reject)=>{
        fs.readFile(obj.fileName, (err, data)=>{
            if(err){
                reject(err);
            }
            obj.data = data.toString();
            resolve(obj);
        });
    });
}


function parseScript(obj){
    obj.parseObj = esprima.parseModule(obj.data, {range: true});
    return obj;
}

function getTopLevel(obj){
    obj.topLevel = [];
    var body = obj.parseObj.body
    body.forEach((node)=>{
        if(node.type === 'VariableDeclaration'){
            obj.topLevel.push({
                name: node.declarations[0].id.name,
                start: node.range[0],
                end: node.range[1],
            });
        } else if(node.type === 'FunctionDeclaration'){
            obj.topLevel.push({
                name: node.id.name,
                start: node.range[0],
                end: node.range[1],
            });
        } else {
            obj.topLevel.push({
                name: node.type,
                start: node.range[0],
                end: node.range[1],
            });
        }
    });
    return obj;
}





app.get('/', (req, res) => res.send('Hello World!'))
app.post('/parse', (req, res)=>{
    console.log(req.body)
    Promise.resolve({fileName: req.body.fileName})
    .then(readFile)
    .then(parseScript)
    .then(getTopLevel)
    .then((obj)=>{
        res.json(obj.topLevel);
    }).catch((e)=>{
        res.send(e);
    });
})

app.listen(3000, () => console.log('Example app listening on port 3000!'))