let format = (...args)=> args[0].replace(/{(\d+)}/g, (_, number)=> typeof args[number] != 'undefined'? args[number] : '');

for(let elem of a){
    for(let desc of elem.desc){
        desc.text = format(desc.param_desc,...desc.param_desc_args)
        delete desc.param_desc;
        delete desc.param_desc_args;
    }
}
JSON.stringify(a.sort(({id:x},{id:y})=> x - y))