using Pscx.IO.ImageMastering;

namespace Pscx.Commands.IO.ImageMastering
{
    public static class ImapiProfileTypeMappings
    {
        public static string GetProfileName(int profile)
        {
            switch (profile)
            {
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_INVALID:
                    {
                        return "INVALID";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_NON_REMOVABLE_DISK:
                    {
                        return "NON_REMOVABLE_DISK";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_REMOVABLE_DISK:
                    {
                        return "REMOVABLE_DISK";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_MO_ERASABLE:
                    {
                        return "MO_ERASABLE";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_MO_WRITE_ONCE:
                    {
                        return "MO_WRITE_ONCE";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_AS_MO:
                    {
                        return "AS_MO";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_CDROM:
                    {
                        return "CDROM";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_CD_RECORDABLE:
                    {
                        return "CD_RECORDABLE";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_CD_REWRITABLE:
                    {
                        return "CD_REWRITABLE";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVDROM:
                    {
                        return "DVDROM";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVD_DASH_RECORDABLE:
                    {
                        return "DVD_DASH_RECORDABLE";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVD_RAM:
                    {
                        return "DVD_RAM";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVD_DASH_REWRITABLE:
                    {
                        return "DVD_DASH_REWRITABLE";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVD_DASH_RW_SEQUENTIAL:
                    {
                        return "DVD_DASH_RW_SEQUENTIAL";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVD_DASH_R_DUAL_SEQUENTIAL:
                    {
                        return "DVD_DASH_R_DUAL_SEQUENTIAL";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVD_DASH_R_DUAL_LAYER_JUMP:
                    {
                        return "DVD_DASH_R_DUAL_LAYER_JUMP";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVD_PLUS_RW:
                    {
                        return "DVD_PLUS_RW";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVD_PLUS_R:
                    {
                        return "DVD_PLUS_R";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVD_PLUS_RW_DUAL:
                    {
                        return "DVD_PLUS_RW_DUAL";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DVD_PLUS_R_DUAL:
                    {
                        return "DVD_PLUS_R_DUAL";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_BD_ROM:
                    {
                        return "BD_ROM";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_BD_R_SEQUENTIAL:
                    {
                        return "BD_R_SEQUENTIAL";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_BD_R_RANDOM_RECORDING:
                    {
                        return "BD_R_RANDOM_RECORDING";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_BD_REWRITABLE:
                    {
                        return "BD_REWRITABLE";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_HD_DVD_ROM:
                    {
                        return "HD_DVD_ROM";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_HD_DVD_RECORDABLE:
                    {
                        return "HD_DVD_RECORDABLE";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_HD_DVD_RAM:
                    {
                        return "HD_DVD_RAM";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DDCDROM:
                    {
                        return "DDCDROM";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DDCD_RECORDABLE:
                    {
                        return "DDCD_RECORDABLE";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_DDCD_REWRITABLE:
                    {
                        return "DDCD_REWRITABLE";
                    }
                case ImapiProfileTypes.IMAPI_PROFILE_TYPE_NON_STANDARD:
                    {
                        return "NON_STANDARD";
                    }
                default:
                    return "Unrecognized profile.";
            }
        }
    }
}