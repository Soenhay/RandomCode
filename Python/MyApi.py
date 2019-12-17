from lib.ApiHelper import *
from lib.JsonConvert import JsonConvert

#TODO get from config file.
urlBaseProd = 'https://api.mysite.com/api/v1/'
urlBaseDev = 'https://apidev.mysite.com/api/v1/'
urlBaseDebug = 'https://localhost:44369/'
urlBase = urlBaseDev

class MyApi(object):
    def UserCreate(authToken, content):
        url = urlBase + 'UserCreate'
        ApiHelper.do_request(RequestType.POST, url, authToken, content)
