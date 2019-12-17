package lib;

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import model.ApiResponse;
import model.AppConfig;
import model.IApiResponse;
import org.apache.http.HttpHeaders;
import org.apache.http.HttpStatus;
import org.apache.http.entity.ContentType;
import org.asynchttpclient.*;
import org.asynchttpclient.uri.Uri;
import org.asynchttpclient.util.HttpConstants;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import java.lang.UnsupportedOperationException;

import java.lang.invoke.MethodHandles;
import java.time.Duration;
import java.time.Instant;
import java.util.Arrays;
import java.util.List;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;

public final class ApiHelper {
    private static final Logger logger = LoggerFactory.getLogger(MethodHandles.lookup().lookupClass());

    static int timeout = 10;
    static DefaultAsyncHttpClientConfig.Builder clientBuilder = Dsl.config()
            .setConnectTimeout(timeout * 1000)
            .setRequestTimeout(timeout * 1000)
            //.setEnabledProtocols(new String[] {"TLSv1.2", "TLSv1.1"}
    ;

    private static AsyncHttpClient client = Dsl.asyncHttpClient(clientBuilder);

    private static volatile Instant ExceptionDetailsTime = Instant.now();
    private static volatile int ExceptionDetailsCounter = 0;

    public static List<Integer> SuccessStatusCodes = Arrays.asList(
        HttpStatus.SC_OK
        , HttpStatus.SC_CREATED
        , HttpStatus.SC_ACCEPTED
        , HttpStatus.SC_NON_AUTHORITATIVE_INFORMATION
        , HttpStatus.SC_NO_CONTENT
        , HttpStatus.SC_RESET_CONTENT
        , HttpStatus.SC_PARTIAL_CONTENT
        , HttpStatus.SC_MULTI_STATUS
    );

    public enum REQUEST_TYPE {
        POST{
            public String toString(){
                return HttpConstants.Methods.POST;
            }
        },
        PUT{
            public String toString(){
                return HttpConstants.Methods.PUT;
            }
        },
        DELETE{
            public String toString(){
                return HttpConstants.Methods.DELETE;
            }
        },
        GET{
            public String toString(){
                return HttpConstants.Methods.GET;
            }
        }
    }

    public static Uri UriGet(String baseUrl, String endpoint) {
        //org.apache.http.client.utils
//        URIBuilder uriBuilder;
//        //java.net
//        URI uriTemp = null;
//        try {
//            uriBuilder = new URIBuilder(baseUrl);
//            String path = Paths.get(uriBuilder.getPath(), endpoint).toString();
//            uriTemp = uriBuilder.setPath(path)
//                    .build()
//                    .normalize();
//        } catch (URISyntaxException e) {
//            e.printStackTrace();
//        }
//        //org.asynchttpclient.uri.
//        Uri uri = Uri.create(uriTemp.toString());
        Uri uri = Uri.create(baseUrl + ("/" + endpoint).replace("//", "/"));
        return uri;
    }

    public static <T> ApiResponse<T> doRequest(TypeToken jsonObjReturnType, REQUEST_TYPE requestType, Uri uri, String authToken) {
        return doRequest(jsonObjReturnType, requestType, uri, authToken, "");
    }

    public static <T> ApiResponse<T> doRequest(TypeToken jsonObjReturnType, REQUEST_TYPE requestType, Uri uri, String authToken, String content) {
        return doRequest(jsonObjReturnType, requestType, uri, authToken, content, ContentType.APPLICATION_JSON);
    }

    public static <T> ApiResponse<T> doRequest(TypeToken jsonObjReturnType, REQUEST_TYPE requestType, Uri uri, String authToken, String content, ContentType contentType) {
        ApiResponse result = new ApiResponse<T>();
        result.IsSuccess = false;

        try {
            String responseString = "";
            Request request = GetRequest(requestType, uri, authToken, content, contentType);
            Future<Response> responseFuture = client.executeRequest(request);
            Response response = responseFuture.get();

            responseString = response.getResponseBody();
            int statusCode = response.getStatusCode();
            result.StatusCode = statusCode;

            if (SuccessStatusCodes.contains(statusCode)) {
                //It was successful.
                result.ReturnMessage = responseString;
                //Type collectionType = new TypeToken<T>(){}.getType();//https://stackoverflow.com/questions/18397342/deserializing-generic-types-with-gson
                //if(jsonObjReturnType.getType().getClass().isAssignableFrom(String.class)) {

                if (String.class.getTypeName().equals(jsonObjReturnType.toString())) {
                    //Gson.fromJson doesn't like trying to convert the object to a string so just save it as a string.
                    result.Data = responseString;
                } else {
                    result.Data = new Gson().fromJson(responseString, jsonObjReturnType.getType());
                }

                if (!result.ReturnMessage.toLowerCase().contains("fail")
                        && !result.ReturnMessage.toLowerCase().contains("error")) {
                    result.IsSuccess = true;
                }
            }  else {
                //It was unsuccessful.
                result.ReturnMessage = String.format("Failed: StatusCode: %d, ResponseString: %s", statusCode, responseString);
            }

            if(!result.IsSuccess) {
                logger.info(String.format("doRequest IsSuccess:%s, StatusCode: %d, Url:%s, ResponseString:%s", result.IsSuccess, statusCode, uri.toString(), responseString));
            }

        } catch (Exception ex) {
            logger.error("doRequest Failed with exception:", ex);
        }

        return result;
    }

    public static <T> void doRequestAsync(TypeToken jsonObjReturnType, REQUEST_TYPE requestType, Uri uri, String authToken, IApiResponse<T> responseHandler) {
        doRequestAsync(jsonObjReturnType, requestType, uri, authToken, responseHandler, "");
    }

    public static <T> void doRequestAsync(TypeToken jsonObjReturnType, REQUEST_TYPE requestType, Uri uri, String authToken, IApiResponse<T> responseHandler, String content) {
        doRequestAsync(jsonObjReturnType, requestType, uri, authToken, responseHandler, content, ContentType.APPLICATION_JSON);
    }

    public static <T> void doRequestAsync(TypeToken jsonObjReturnType, REQUEST_TYPE requestType, Uri uri, String authToken, IApiResponse<T> responseHandler, String content, ContentType contentType) {
        try {
            Request request = GetRequest(requestType, uri, authToken, content, contentType);
            ListenableFuture<Response> listenableFuture = client.executeRequest(request);
            listenableFuture.addListener(() -> {
                ApiResponse result = new ApiResponse<T>();
                result.IsSuccess = false;
                Response response = null;
                try {
                    response = listenableFuture.get();
                } catch (InterruptedException e) {
                    logger.error("doRequestAsync InterruptedException: ", e);
                } catch (ExecutionException e) {
                    //logger.error("doRequestAsync ExecutionException: ", e);//Full stack trace. Not sure why but this line causes problems.
                    logger.error("doRequestAsync ExecutionException: " + (++ExceptionDetailsCounter));
                }

                String responseString = response.getResponseBody();
                int statusCode = response.getStatusCode();
                result.StatusCode = statusCode;

                if (SuccessStatusCodes.contains(statusCode)) {
                    //It was successful.
                    result.ReturnMessage = responseString;
                    //Type collectionType = new TypeToken<T>(){}.getType();//https://stackoverflow.com/questions/18397342/deserializing-generic-types-with-gson
                    //if(jsonObjReturnType.getType().getClass().isAssignableFrom(String.class)) {

                    if (String.class.getTypeName().equals(jsonObjReturnType.toString())) {
                        //Gson.fromJson doesn't like trying to convert the object to a string so just save it as a string.
                        result.Data = responseString;
                    } else {
                        result.Data = new Gson().fromJson(responseString, jsonObjReturnType.getType());
                    }

                    if (!result.ReturnMessage.toLowerCase().contains("fail")
                            && !result.ReturnMessage.toLowerCase().contains("error")) {
                        result.IsSuccess = true;
                    }
                } else {
                    //It was unsuccessful.
                    result.ReturnMessage = String.format("Failed: StatusCode: %d, ResponseString: %s", statusCode, responseString);
                }

                if(!result.IsSuccess) {
                    logger.info(String.format("doRequestAsync IsSuccess:%s, StatusCode: %d, Url:%s, ResponseString:%s", result.IsSuccess, statusCode, uri.toString(), responseString));
                }

                if(responseHandler != null) {
                    responseHandler.onCompleted(result);
                }
            }, Executors.newCachedThreadPool());

        } catch (Exception ex) {
            logger.error("doGetRequestAsync Failed with exception:", ex);
        }
    }

    private static Request GetRequest(REQUEST_TYPE requestType, Uri uri, String authToken, String content, ContentType contentType) {
        RequestBuilder builder = null;

        switch (requestType) {
            default:
                break;
            case GET:
                builder = new RequestBuilder(HttpConstants.Methods.GET)
                        //.setHeader(HttpHeaders.ACCEPT, ContentType.APPLICATION_JSON.toString())
                        .setHeader(HttpHeaders.CONTENT_TYPE, ContentType.APPLICATION_JSON.toString());
                break;
            case POST:
                builder = new RequestBuilder(HttpConstants.Methods.POST)
                        //.setHeader(HttpHeaders.ACCEPT, ContentType.APPLICATION_JSON.toString())
                        .setHeader(HttpHeaders.CONTENT_TYPE, ContentType.APPLICATION_JSON.toString())
                        .setBody(content);
                break;
            case PUT:
                if (contentType == ContentType.MULTIPART_FORM_DATA) {
                    throw new UnsupportedOperationException();
                } else if (contentType == ContentType.APPLICATION_JSON) {
                    builder = new RequestBuilder(HttpConstants.Methods.PUT)
                            .setHeader(HttpHeaders.CONTENT_TYPE, ContentType.APPLICATION_JSON.toString())
                            .setBody(content);
                }
                break;
            case DELETE:
                builder = new RequestBuilder(HttpConstants.Methods.DELETE);
                break;
        }

        builder.setUri(uri);

        if(uri.toString().toLowerCase().contains("tamucc") || uri.toString().toLowerCase().contains("localhost") ) {
            builder.setHeader("AppName", AppConfig.getInstance().AppName);
        }

        if(authToken != null && !authToken.isEmpty()) {
            builder.setHeader(HttpHeaders.AUTHORIZATION, "Bearer " + authToken);
        }

        return builder.build();
    }

//    private static boolean OkToLogException() {
////        if (OkToLogException()) {
////            logger.error("doRequestAsync ExecutionException: ", e);//Full stack trace. Limited to 5 times in a 5 second interval
////        } else {
////            logger.error("doRequestAsync ExecutionException: " + uri);//Shorter message.
////        }
//
//        ExceptionDetailsCounter++;
//
//        if (ExceptionDetailsCounter > 5) {
//            ExceptionDetailsCounter = 0;
//        }
//
//        if (Duration.between(ExceptionDetailsTime, Instant.now()).toMillis() > 5000) {
//            ExceptionDetailsTime = Instant.now();
//        }
//
//        return ExceptionDetailsCounter <= 5 && Duration.between(ExceptionDetailsTime, Instant.now()).toMillis() <= 5000;
//    }
}
