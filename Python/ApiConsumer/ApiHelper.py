from enum import Enum
import requests
#from typing import TypeVar, Generic
from lib.JsonConvert import JsonConvert

#T = TypeVar('T')


class RequestType(Enum):
    GET = 1
    POST = 2
    PUT = 3


class ApiResponse:
    def __init__(self):
        self.IsSuccess = False
        self.ReturnMessage = ''
        self.Data = None
        self.Status_Code = None


class ApiHelper:
    @staticmethod
    def do_request(requestType, url, authToken, content):
        # type: (RequestType, str, str, str) -> object
        print('do_request for type: {type}, url:{url} , token:{token}'.format(type=requestType, url=url, token=authToken))
        headers = {'content-type': 'application/json',
                   'Authorization': 'bearer ' + authToken}

        response = {
            RequestType.GET: lambda x, y, z:(requests.get(x, headers=y)),
            RequestType.POST: lambda x, y, z: requests.post(x, headers=y, data=z),
            RequestType.PUT: lambda x, y, z: requests.put(x, headers=y, data=z)
        }[requestType](url, headers, content)

        #https://www.restapitutorial.com/httpstatuscodes.html
        #https://github.com/psf/requests/blob/master/requests/status_codes.py
        successCodes = (
            requests.codes.ok,                      #200
            requests.codes.created,                 #201
            requests.codes.accepted,                #202
            requests.codes.non_authoritative_info,  #203
            requests.codes.no_content,              #204
            requests.codes.reset_content,           #205
            requests.codes.partial,                 #206
            requests.codes.multi_status,            #207
            requests.codes.already_reported,        #208
            requests.codes.im_used,                 #226
            )

        if response is None:
            print('Response is null')
        else:
            print('Get Response...')
            result = ApiResponse()
            result.ReturnMessage = response.text
            result.Status_Code = response.status_code
            if response.status_code in successCodes:
                result.IsSuccess = True
                result.Data = JsonConvert.FromJSON(response.text)
            else:
                result.IsSuccess = False
                result.Data = None

        print('do_request IsSuccess:{success}'.format(success=result.IsSuccess))
        return result

