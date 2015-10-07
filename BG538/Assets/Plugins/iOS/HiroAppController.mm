#import "UnityAppController.h"

@interface HiroAppController : UnityAppController
{
}
@end
@implementation HiroAppController
- (void) applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    [super applicationDidReceiveMemoryWarning:application];
    UnitySendMessage("GLResourceManager", "_iOS_ReceivedMemoryWarning","");
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(HiroAppController)

