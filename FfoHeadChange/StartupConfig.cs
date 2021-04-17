using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FfoHeadChange
{
    public class StartupConfig
    {
        private static readonly string _PATH_PREFIX = @"";
        public static readonly string SERVANT_DB_PART_FILE = _PATH_PREFIX + "Resource/Db/ServantDb-Parts.txt";
        public static readonly string SERVANT_DB_LOCALIZE_FILE = _PATH_PREFIX + "Resource/Db/ServantDb-Localize.txt";
        public static readonly string ICON_DIRECTORY = _PATH_PREFIX + "Resource/Icon";
        public static readonly string HEAD_DIRECTORY = _PATH_PREFIX + "Resource/Head";
        public static readonly string BODY_DIRECTORY = _PATH_PREFIX + "Resource/Body";
        public static readonly string BACKGROUND_DIRECTORY = _PATH_PREFIX + "Resource/Background";
        public static readonly string ICON_PARSE_PATTERN = "^icon_servant_(\\d{3}).png$";
    }
}
