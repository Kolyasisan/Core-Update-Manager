# Grand Update Manager

An optimized, generally garbage-free Update Manager that works with Arrays, first used for Grand Dad Mania Revived (https://pechka-productions.itch.io/grand-dad-mania). If you're asking "why" then read this: https://blogs.unity3d.com/2015/12/23/1k-update-calls/

Long story short: having "magic" methods called from Unity has an overhead and thus, if you wish to gain performance or get more freedom/features at the expense of being a little more careful, you can write your own Update Manager which will be the only thing that receives the magic Update method and distributes it across all the other objects. In Grand Dad Mania Revived we managed to save 0.5ms on average on Xiaomi Mi5S Plus.

Simply derive your MonoBehaviour from OverridableMonoBehaviour and use public override void UpdateMe variants. If you use Start methods then change them to Protected Override Void Start and call base.Start() in them.
Garbage is generetad only upon Arrays overflow when trying to add more scripts than the array can hold.
Arrays are used and managed manually instead of lists for maximum performance.
See detailed instructions in the UpdateManager script.

Warning: may potentially enforce you to facepalm yourself upon seeing the code!

Please read the License.
