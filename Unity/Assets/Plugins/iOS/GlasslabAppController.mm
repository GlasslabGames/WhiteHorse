#import "UnityAppController.h"

@interface GlasslabAppController : UnityAppController
{
}
@end
@implementation GlasslabAppController
- (void) applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    [super applicationDidReceiveMemoryWarning:application];
    UnitySendMessage("GLResourceManager", "_iOS_ReceivedMemoryWarning","");
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(GlasslabAppController)

