/// @file
/// @copyright  Copyright (c) 2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System.IO;
using System.Text;

namespace I18N.DotNet.Test
{
    /// <summary>
    /// Helper methods for tests.
    /// </summary>
    public static class TestHelpers
    {
        public static Stream CreateStream( string data )
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter( stream, Encoding.UTF8, 1024, true );

            writer.Write( data );
            writer.Flush();
            stream.Seek( 0, SeekOrigin.Begin );

            return stream;
        }

        public static Stream GetI18NConfig()
        {
            string config =
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <Key>Simple Key 1</Key>\n" +
                "    <Value lang='es'>Clave simple 1</Value>\n" +
                "    <Value lang='es-AR'>Che, viste, clave resimple 1. Obvio</Value>\n" +
                "    <Value lang='fr'>Clef simple 1</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Simple Key 2</Key>\n" +
                "    <Value lang='es'>Clave simple 2</Value>\n" +
                "    <Value lang='fr'>Clef simple 2</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Simple Key 3</Key>\n" +
                "    <Value lang='es'>Clave simple 3</Value>\n" +
                "    <Value lang='fr'>Clef simple 3</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Simple Key 4</Key>\n" +
                "    <Value lang='es'>Clave simple 4</Value>\n" +
                "    <Value lang='fr'>Clef simple 4</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Format Key: {0:X4}</Key>\n" +
                "    <Value lang='es'>Clave de formato: {0}</Value>\n" +
                "    <Value lang='fr'>Clef de format: {0}</Value>\n" +
                "  </Entry>\n" +
                "  <Context id='Level1.Level2'>\n" +
                "    <Entry>\n" +
                "      <Key>Simple Key 1</Key>\n" +
                "      <Value lang='es'>Clave simple 1 en contexto L2</Value>\n" +
                "      <Value lang='fr'>Clef simple 1 en contexte L2</Value>\n" +
                "    </Entry>\n" +
                "  </Context>\n" +
                "  <Context id='Level1'>\n" +
                "    <Entry>\n" +
                "      <Key>Simple Key 2</Key>\n" +
                "      <Value lang='es'>Clave simple 2 en contexto L1</Value>\n" +
                "      <Value lang='fr'>Clef simple 2 en contexte L1</Value>\n" +
                "    </Entry>\n" +
                "    <Context id='Level2'>\n" +
                "      <Entry>\n" +
                "        <Key>Simple Key 3</Key>\n" +
                "        <Value lang='fr'>Clef simple 3 en contexte L2</Value>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "    <Key>Escaped:\\n\\r\\f&amp;\\t\\v\\b\\\\n\\xABC</Key>\n" +
                "    <Value lang='es'>Escapado:\\n\\r\\f&amp;\\t\\v\\b\\\\n\\xABC</Value>\n" +
                "  </Entry>\n" +
                "</I18N>";

            return CreateStream( config );
        }

    }
}
