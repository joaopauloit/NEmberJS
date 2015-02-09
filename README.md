

# NEmberJS
ASP NET Web Api and EmberJS Integration.

This Project was forked from [emvelope](https://github.com/jonnii/emvelope). I just created a new project instead send PR because I did something diferent. For example, emvelope by default return name properties in n json using snake_case conventions instead camelCase. Anyway, below follow some behaviors in NEmberJS:



* Return json as expected in ember-data(RESTAdapter) [json conventions] (http://emberjs.com/guides/models/the-rest-adapter/#toc_json-conventions)
* Return Enum as int
* Error class to add custom/domain validations as described in [DS.errors](http://emberjs.com/api/data/classes/DS.Errors.html).
* Validation Attribute when you use [Required] in your view models.



```
//if you want to use ValidateModelActionFilter for all requests
configuration.Filters.Add(new ValidateModelActionFilerAttribute());

configuration.Formatters.Insert(0, new NEmberJSMediaTypeFormatter());
```


##Display message errors in EmberJS

```
            post.save().then(function(){
                self.transitionToRoute('posts');
            }
            , function(data){
                var errors = data.responseJSON.errors;
                self.set('errors', errors);
            });
```


#Under construction
