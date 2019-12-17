package lib;

import Main.Main;
import com.google.gson.reflect.TypeToken;
import org.apache.commons.lang3.StringUtils;
import org.asynchttpclient.uri.Uri;

import java.io.File;
import java.net.URISyntaxException;
import java.security.CodeSource;

public final class Common {
    private static  String jarDir;
    public static String GetJarDirectory() {
        if(StringUtils.isEmpty(jarDir)) {
            CodeSource codeSource = Main.class.getProtectionDomain().getCodeSource();
            File jarFile = null;
            try {
                jarFile = new File(codeSource.getLocation().toURI().getPath());
            } catch (URISyntaxException e) {
                e.printStackTrace();
            }
            jarDir = jarFile.getParentFile().getPath();
        }

        return jarDir;
    }

    public static boolean CheckInternetConnection() {
        Uri uri = Uri.create("https://www.google.com");
        TypeToken<String> typeToken = new TypeToken<String>() {};
        int statusCode = ApiHelper.doRequest(typeToken, ApiHelper.REQUEST_TYPE.GET, uri, null).StatusCode;
        return ApiHelper.SuccessStatusCodes.contains(statusCode);
    }
}
