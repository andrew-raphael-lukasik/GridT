// Wrote by Andrew Raphael Lukasik 
// https://twitter.com/andrewlukasik
// MIT license
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

//[System.Serializable]//Wont work, see workaround at the bottom//nie działa, trzeba obchodzić (patrz na samym dole)
public class Grid <T> : Disposable, ICollection, IEnumerable<T>
{
    #region FIELDS_&_PROPERTIES

    [SerializeField] protected T[] _values;
    /// <summary> Internal 1d data array </summary>
    public T[] values { get { return _values; } }

    [SerializeField] int _width;
    public int width { get { return _width; } }

    [SerializeField] int _height;
    public int height { get { return _height; } }

    public int Length { get { return _width*_height; } }

    #endregion
    #region CONSTRUCTORS

    public Grid ( int width , int height )
    {
        this._values = new T[width*height];
        this._width = width;
        this._height = height;
    }

    #endregion
    #region OPERATORS

    public T this [ int index1d ]
    {
        get { return _values[ index1d ]; }
        set { _values[ index1d ] = value; }
    }

    public T this [ int x , int y ]
    {
        get { return _values[ Index2dTo1d( x , y ) ]; }
        set { _values[ Index2dTo1d( x , y ) ] = value; }
    }

    /// <summary>
    /// Fill rect
    /// </summary>
    public T this [ int x , int y , int w , int h ]
    {
        set { this.FillRect( x , y , w , h , value ); }
    }

    /// <summary>
    /// Is expression true for EVERY field in rect?
    /// </summary>
    public bool this [ int x , int y , int w , int h , System.Predicate<T> predicate , bool debug = false ]
    {
        get { return this.TrueForEvery( x , y , w , h , predicate , debug ); }
    }

    #endregion
    #region disposable_implementationd

    /// <summary> Free other managed objects that implement IDisposable only </summary>
    protected override void DisposeManaged ()
    {
        if( typeof(Disposable).IsAssignableFrom( typeof(T) ) )
        {
            for( int i = 0 ; i < _values.GetLength( 0 ) ; i++ )
            {
                Disposable d = _values[ i ] as Disposable;
                if( d != null )
                {
                    d.Dispose();
                }
            }
        }
    }

    /// <summary> Release any unmanaged objects set the object references to null </summary>
    protected override void DisposeUnmanaged ()
    {
        if( typeof(T) is System.Object )
        {
            int arrayLength = this._values.Length;
            for( int i = 0 ; i < arrayLength ; i++ )
            {
                _values[ i ] = default(T);
            }
        }
    }

    #endregion
    #region ICollection implementation

    void ICollection.CopyTo ( System.Array array , int index )
    {
        _values.CopyTo( array , index );
    }

    int ICollection.Count { get { return _values.Length; } }

    object ICollection.SyncRoot { get { throw new System.NotImplementedException(); } }

    bool ICollection.IsSynchronized { get { throw new System.NotImplementedException(); } }

    #endregion
    #region IEnumerable implementation

    IEnumerator<T> IEnumerable<T>.GetEnumerator ()
    {
        return _values.GetEnumerator() as IEnumerator<T>;
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
        return _values.GetEnumerator();
    }

    #endregion
    #region PRIVATE_METHODS



    #endregion
    #region PUBLIC_METHODS

    public override int GetHashCode ()
    {
        int hash = 17;
        int arrayLength = this._values.Length;

        if( arrayLength > 0 && typeof(T) is System.Object )
        {
            
            for( int i = 0 ; i < arrayLength ; i++ )
            {
                T t = _values[ i ];
                hash = hash*23+( t == null ? 0 : t.GetHashCode() );
            }

        } else
        {

            for( int i = 0 ; i < arrayLength ; i++ )
            {
                T t = _values[ i ];
                hash = hash*23+t.GetHashCode();
            }

        }

        hash = hash*23+_width.GetHashCode();
        hash = hash*23+_height.GetHashCode();

        return hash;
    }

    /// <summary> Executes for each grid cell </summary>
    public void ForEach ( System.Action<T> action )
    {
        int arrayLength = this._values.Length;
        for( int i = 0 ; i < arrayLength ; i++ )
        {
            action( this._values[ i ] );
        }
    }
    public void ForEach ( System.Predicate<T> predicate , System.Action<T> action )
    {
        int arrayLength = this._values.Length;
        for( int i = 0 ; i < arrayLength ; i++ )
        {
            T cell = this._values[ i ];
            if( predicate( cell ) == true )
            {
                action( cell );
            }
        }
    }
    public void ForEach ( System.Func<T> func )
    {
        int arrayLength = this._values.Length;
        for( int i = 0 ; i < arrayLength ; i++ )
        {
            this._values[ i ] = func();
        }
    }
    /// <param name="func"> argument is grid's index1d </param>
    public void ForEach ( System.Action<int> action )
    {
        int arrayLength = this._values.Length;
        for( int i = 0 ; i < arrayLength ; i++ )
        {
            action( i );
        }
    }
    /// <param name="func"> arguments are grid's index2d </param>
    public void ForEach ( System.Action<int,int> action )
    {
        for( int x = 0 ; x < _width ; x++ )
        {
            for( int y = 0 ; y < _height ; y++ )
            {
                action( x , y );
            }
        }
    }

    /// <summary>
    /// For each in rectangle
    /// </summary>
    /// <param name="func"> parameters will be grid's X and Y 2d indexes </param>
    public void ForEach ( int x , int y , int w , int h , System.Action<int,int> action , System.Action<int,int> onRectIsOutOfBounds = null )
    {
        int yStart = y;
        int xEnd = x+w;
        int yEnd = y+h;
        if( onRectIsOutOfBounds != null && ( xEnd > _width || yEnd > _height ) )
        {
            onRectIsOutOfBounds( x , y );
        } else
        {
            for( ; x < xEnd ; x++ )
            {
                //Debug.Log( "\t\t\tx="+x );
                for( ; y < yEnd ; y++ )
                {
                    //Debug.Log( "\t\t\ty="+y );
                    action( x , y );
                }
                y = yStart;
            }
        }
        //Debug.Log( "\t\t\tended with xy: "+x+" "+y );
    }
    public void ForEach ( int x , int y , int w , int h , System.Func<T,T> func )
    {
        ForEach(
            x , y , w , h ,
            (int ax , int ay ) =>
            {
                ( this )[ ax , ay ] = func( ( this )[ ax , ay ] );
            }
        );
    }
    /// <param name="func"> parameters will be grid's 1d index </param>
    public void ForEach ( int x , int y , int w , int h , System.Action<int> action )
    {
        ForEach(
            x , y , w , h ,
            (int ax , int ay ) =>
            {
                action( Index2dTo1d( ax , ay ) );
            }
        );
    }

    /// <summary>
    /// Converts 2d to 1d array index
    /// </summary>
    public int Index2dTo1d ( int x , int y )
    {
        #if DEBUG
        if( IsIndexValid( x , y ) == false )
        {
            Debug.LogWarning( string.Format( "[{0},{1}] index is invalid for this grid" , x , y ) );
        }
        #endif
        return y*_width+x;
    }

    /// <summary>
    /// Converts 1d to 2d array index
    /// </summary>
    public Vector2 Index1dTo2d ( int i )
    {
        return new Vector2(
            i%_width ,
            i/_width
        );
    }

    /// <summary> Determines whether index is valid for this grid ie. inside array bounds </summary>
    public bool IsIndexValid ( int x , int y )
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }

    /// <summary> Transforms local position to cell index </summary>
    public bool LocalPointToIndex2d ( Vector3 localPoint , float spacing , out Vector2 result )
    {
        int x = (int)( ( localPoint.x+(float)_width*0.5f*spacing )/spacing );
        int z = (int)( ( localPoint.z+(float)_height*0.5f*spacing )/spacing );
        if( IsIndexValid( x , z ) )
        {
            result = new Vector2( x , z );
            return true;
        } else
        {
            result = new Vector2( -1f , -1f );
            return false;
        }
    }

    /// <summary>
    /// Transforms index to local position.
    /// </summary>
    public Vector3 IndexToLocalPoint ( int x , int y , float spacing )
    {
        return new Vector3(
            ( (float)x*spacing )+( -_width*spacing*0.5f )+( spacing*0.5f ) ,
            0f ,
            ( (float)y*spacing )+( -_height*spacing*0.5f )+( spacing*0.5f )
        );
    }
    public Vector3 IndexToLocalPoint ( int index1d , float spacing )
    {
        Vector2 index2d = Index1dTo2d( index1d );
        return new Vector3(
            ( index2d.x*spacing )+( -_width*spacing*0.5f )+( spacing*0.5f ) ,
            0f ,
            ( index2d.y*spacing )+( -_height*spacing*0.5f )+( spacing*0.5f )
        );
    }

    /// <returns>
    /// Rect center position 
    /// </returns>
    public Vector3 IndexToLocalPoint ( int x , int y , int w , int h , float spacing )
    {
        Vector3 cornerA = IndexToLocalPoint( x , y , spacing );
        Vector3 cornerB = IndexToLocalPoint( x+w-1 , y+h-1 , spacing );
        return cornerA+( cornerB-cornerA )*0.5f;
    }

    /// <summary>
    /// Gets the surrounding type count.
    /// </summary>
    public int GetSurroundingTypeCount ( int x , int y , System.Predicate<T> predicate )
    {
        int result = 0;
        for( int neighbourX = x-1 ; neighbourX <= x+1 ; neighbourX++ )
        {
            for( int neighbourY = y-1 ; neighbourY <= y+1 ; neighbourY++ )
            {
                if( neighbourX >= 0 && neighbourX < this.width && neighbourY >= 0 && neighbourY < this.height )
                {
                    if( neighbourX != x || neighbourY != y )
                    {
                        result += predicate( ( this )[ neighbourX , neighbourY ] ) ? 1 : 0;
                    }
                } else
                {
                    result++;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Gets the surrounding field values
    /// </summary>
    /// <returns>
    /// 8-bit long clockwise formatted bit values 
    /// 7 0 1           [x-1,y+1]  [x,y+1]  [x+1,y+1]
    /// 6   2      ==   [x-1,y]     [x,y]     [x+1,y]
    /// 5 4 3           [x-1,y-1]  [x,y-1]  [x+1,y-1]
    /// for example: 1<<0 is top, 1<<1 is top-right, 1<<2 is right, 1<<6|1<<4|1<<2 is both left,down and right
    /// </returns>
    public string GetMarchingSquares ( int x , int y , System.Predicate<T> predicate )
    {
        int result = 00000000;

        //out of bounds test:
        bool xPlus = x+1 < _width;
        bool yPlus = y+1 < _height;
        bool xMinus = x-1 >= 0;
        bool yMinus = y-1 >= 0;

        //top, down:
        result += yPlus && predicate( this[ x , y+1 ] ) ? 10000000 : 0;
        result += yMinus && predicate( this[ x , y-1 ] ) ? 00001000 : 0;

        //right side:
        result += xPlus && yPlus && predicate( this[ x+1 , y+1 ] ) ? 01000000 : 0;
        result += xPlus && predicate( this[ x+1 , y ] ) ? 00100000 : 0;
        result += xPlus && yMinus && predicate( this[ x+1 , y-1 ] ) ? 00010000 : 0;

        //left side:
        result += xMinus && yPlus && predicate( this[ x-1 , y+1 ] ) ? 00000001 : 0;
        result += xMinus && predicate( this[ x-1 , y ] ) == true ? 00000010 : 0;
        result += xMinus && yMinus && predicate( this[ x-1 , y-1 ] ) ? 00000100 : 0;

        return result.ToString( "00000000" );
    }
    public int GetMarchingSquares_BitShifted ( int x , int y , System.Predicate<T> predicate )
    {
        int result = 0;

        //out of bounds test:
        bool xPlus = x+1 < _width;
        bool yPlus = y+1 < _height;
        bool xMinus = x-1 >= 0;
        bool yMinus = y-1 >= 0;

        //top, down:
        result |= yPlus && predicate( this[ x , y+1 ] ) ? 1<<0 : 0;
        result |= yMinus && predicate( this[ x , y-1 ] ) ? 1<<4 : 0;

        //right side:
        result |= xPlus && yPlus && predicate( this[ x+1 , y+1 ] ) ? 1<<1 : 0;
        result |= xPlus && predicate( this[ x+1 , y ] ) ? 1<<2 : 0;
        result |= xPlus && yMinus && predicate( this[ x+1 , y-1 ] ) ? 1<<3 : 0;

        //left side:
        result |= xMinus && yPlus && predicate( this[ x-1 , y+1 ] ) ? 1<<7 : 0;
        result |= xMinus && predicate( this[ x-1 , y ] ) == true ? 1<<6 : 0;
        result |= xMinus && yMinus && predicate( this[ x-1 , y-1 ] ) ? 1<<5 : 0;

        return result;
    }

    /// <summary>
    /// AND operation on cells
    /// </summary>
    public bool TrueForEvery ( int x , int y , int w , int h , System.Predicate<T> predicate , bool debug = false )
    {
        bool result = true;
        ForEach(
            x , y , w , h ,
            (int ax , int ay ) =>
            {

                //debug next field:
                if( debug == true )
                {
                    Debug.Log( "\t\t"+ax+"|"+ay+" (debug = "+debug+")" );
                }

                //evaluate next field:
                if( predicate( ( this )[ ax , ay ] ) == false )
                {
                    result = false;
                    return;
                }

            } ,
            (int ax , int ay ) =>
            {
                
                //debug on out of bounds:
                if( debug == true )
                {
                    Debug.Log( string.Format( "\t\trect[{0},{1},{2},{3}] is out of grid's bounds" , ax , ay , w , h ) );
                }

                //exe on out of bounds:
                result = false;
                return;

            }
        );
        return result;
    }
    /// <summary>
    /// OR operation on cells
    /// </summary>
    public bool TrueForAny ( int x , int y , int w , int h , System.Predicate<T> predicate )
    {
        bool result = false;
        ForEach(
            x , y , w , h ,
            (int ax , int ay ) =>
            {
                if( predicate( ( this )[ ax , ay ] ) == true )
                {
                    result = true;
                    return;
                }
            }
        );
        return result;
    }

    /// <summary>
    /// Smooth operation
    /// NOT TESTED
    /// </summary>
    public void Smooth ( int iterations , System.Predicate<T> countNeighbours , System.Func<T,T> overThreshold , System.Func<T,T> belowThreshold , System.Func<T,T> equalsThreshold , int threshold = 4 )
    {
        for( int i = 0 ; i < iterations ; i++ )
        {
            for( int x = 0 ; x < _width ; x++ )
            {
                for( int y = 0 ; y < _height ; y++ )
                {
                    int neighbourWallTiles = GetSurroundingTypeCount( x , y , countNeighbours );
                    if( neighbourWallTiles > threshold )
                    {
                        ( this )[ x , y ] = overThreshold( ( this )[ x , y ] );
                    } else if( neighbourWallTiles < threshold )
                    {
                        ( this )[ x , y ] = belowThreshold( ( this )[ x , y ] );
                    } else
                    {
                        ( this )[ x , y ] = equalsThreshold( ( this )[ x , y ] );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Fill
    /// </summary>
    public void Fill ( System.Predicate<T> predicate , T fill )
    {
        int arrayLength = this._values.Length;
        for( int i = 0 ; i < arrayLength ; i++ )
        {
            if( predicate( _values[ i ] ) == true )
            {
                _values[ i ] = fill;
            }
        }
    }
    public void Fill ( System.Func<T> fillFunc )
    {
        int arrayLength = this._values.Length;
        for( int i = 0 ; i < arrayLength ; i++ )
        {
            _values[ i ] = fillFunc();
        }
    }
    /// <param name="fillFunc"> int params are x & y cordinates (index 2d)</param>
    public void Fill ( System.Func<int,int,T> fillFunc )
    {
        for( int x = 0 ; x < _width ; x++ )
        {
            for( int y = 0 ; y < _height ; y++ )
            {
                ( this )[ x , y ] = fillFunc( x , y );
            }
        }
    }
    /// <param name="fillFunc"> int param is index 1d </param>
    public void Fill ( System.Func<int,T> fillFunc )
    {
        int arrayLength = this._values.Length;
        for( int i = 0 ; i < arrayLength ; i++ )
        {
            _values[ i ] = fillFunc( i );
        }
    }

    /// <summary>
    /// Fill rectangle
    /// </summary>
    public void FillRect ( int x , int y , int w , int h , T fill )
    {
        ForEach(
            x , y , w , h ,
            (int ax , int ay ) =>
            {
                ( this )[ ax , ay ] = fill;
            }
        );
    }
    public void FillRect ( int x , int y , int w , int h , System.Predicate<T> predicate , T fill )
    {
        ForEach(
            x , y , w , h ,
            (int ax , int ay ) =>
            {
                if( predicate( ( this )[ ax , ay ] ) == true )
                {
                    ( this )[ ax , ay ] = fill;
                }
            }
        );
    }
    public void FillRect ( int x , int y , int w , int h , System.Func<T,T> fillFunc )
    {
        ForEach(
            x , y , w , h ,
            (int ax , int ay ) =>
            {
                ( this )[ ax , ay ] = fillFunc( ( this )[ ax , ay ] );
            }
        );
    }
    /// <param name="fillFunc"> int params are x & y cordinates (index 2d)</param>
    public void FillRect ( int x , int y , int w , int h , System.Func<int,int,T> fillFunc )
    {
        ForEach(
            x , y , w , h ,
            (int ax , int ay ) =>
            {
                ( this )[ ax , ay ] = fillFunc( ax , ay );
            }
        );
    }

    /// <summary>
    /// </summary>
    public void FillBorders ( T fill )
    {

        // fill horizontal border lines:
        int yMax = _height-1;
        for( int x = 0 ; x < _width ; x++ )
        {
            ( this )[ x , 0 ] = fill;
            ( this )[ x , yMax ] = fill;
        }

        // fill vertical border lines:
        int xMax = _width-1;
        for( int y = 1 ; y < _height-1 ; y++ )
        {
            ( this )[ 0 , y ] = fill;
            ( this )[ xMax , y ] = fill;
        }

    }

    #endregion
}

/*
#region UNITY_SERIALIZATION_WORKAROUND_EXAMPLE
[System.Serializable]
public class Grid_Bool : Grid<bool>
{
    public Grid_Bool ( int width , int height ) : base( width , height ) {}
}

then use like:
[SerializeField] Grid_Bool _myGrid = new Grid_Bool( 5 , 10 );//will be visible in inspector

#endregion
*/